using System;
using System.Collections.Generic;
using System.IO;
using BepInEx.Logging;
using Newtonsoft.Json;
using PixelCrushers.DialogueSystem;

namespace LiukNoceda.LittleWitchItalian
{
    /// <summary>
    /// Traduce i dialoghi, che sono il grosso del testo del gioco (48.629 righe,
    /// quasi 2 milioni di caratteri) e non passano da Unity Localization.
    ///
    /// I dialoghi stanno nel Dialogue System di PixelCrushers, con un database per
    /// lingua (`DialogueDB_en`, `DialogueDB_ko`, …) caricato via Addressables. Il
    /// testo **non** e' nel campo `Dialogue Text`, che qui e' quasi sempre vuoto: sta
    /// in un campo che si chiama come il **codice lingua**. Lo si vede in
    /// <c>DialogueEntry.GetCurrentDialogueTextField()</c>:
    ///
    ///     return Field.AssignedField(fields, Localization.language)
    ///            ?? Field.Lookup(fields, "Dialogue Text");
    ///
    /// Con lingua `it` il gioco cerca quindi un campo `it`. La strategia:
    ///
    /// 1. far caricare il database **inglese** anche quando la lingua e' italiana
    ///    (un `DialogueDB_it` non esiste e non lo si puo' creare via Addressables);
    /// 2. aggiungere a ogni voce un campo `it` col testo tradotto.
    ///
    /// **Ogni voce deve avere il campo `it`, tradotto o no.** `AssignedField`
    /// restituisce null se il campo e' vuoto, e il ripiego e' `Dialogue Text`, che in
    /// questo gioco e' vuoto: una voce senza `it` diventa una battuta muta. Percio'
    /// le voci non ancora tradotte ricevono l'inglese, esplicitamente.
    ///
    /// Le altre sezioni seguono la convenzione di PixelCrushers, verificata sul
    /// database coreano: attori `Name ko`, oggetti `Description ko`,
    /// `Success Description ko`, `Failure Description ko`. Per noi con `it`.
    /// </summary>
    internal sealed class Dialoghi
    {
        private const string CampoTesto = "it";
        private const string CodiceModello = "en";

        /// <summary>{id conversazione: {id voce: testo}}</summary>
        private readonly Dictionary<string, Dictionary<string, string>> _conversazioni =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        /// <summary>{id attore: nome}</summary>
        private readonly Dictionary<string, string> _attori =
            new Dictionary<string, string>(StringComparer.Ordinal);

        /// <summary>{id oggetto: {campo: testo}}</summary>
        private readonly Dictionary<string, Dictionary<string, string>> _oggetti =
            new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

        private readonly ManualLogSource _log;

        internal Dialoghi(ManualLogSource log) => _log = log;

        internal int VociTradotte { get; private set; }
        internal bool Applicato { get; private set; }

        // ------------------------------------------------------------------
        // Lettura dei file
        // ------------------------------------------------------------------

        private sealed class Blocco
        {
            public Dictionary<string, Dictionary<string, string>> conversazioni;
            public Dictionary<string, string> attori;
            public Dictionary<string, Dictionary<string, string>> oggetti;
        }

        internal void Carica(string cartella)
        {
            if (!Directory.Exists(cartella))
            {
                _log.LogInfo($"Nessuna cartella dialoghi ({Path.GetFileName(cartella)}): "
                             + "i dialoghi resteranno in inglese");
                return;
            }

            int file = 0;
            foreach (string percorso in Directory.GetFiles(cartella, "*.json"))
            {
                try
                {
                    var b = JsonConvert.DeserializeObject<Blocco>(File.ReadAllText(percorso));
                    if (b == null) continue;

                    if (b.conversazioni != null)
                        foreach (var c in b.conversazioni)
                        {
                            if (!_conversazioni.TryGetValue(c.Key, out var voci))
                                _conversazioni[c.Key] = voci = new Dictionary<string, string>(StringComparer.Ordinal);
                            foreach (var v in c.Value)
                                if (!string.IsNullOrEmpty(v.Value)) voci[v.Key] = v.Value;
                        }

                    if (b.attori != null)
                        foreach (var a in b.attori)
                            if (!string.IsNullOrEmpty(a.Value)) _attori[a.Key] = a.Value;

                    if (b.oggetti != null)
                        foreach (var o in b.oggetti)
                        {
                            if (!_oggetti.TryGetValue(o.Key, out var campi))
                                _oggetti[o.Key] = campi = new Dictionary<string, string>(StringComparer.Ordinal);
                            foreach (var c in o.Value)
                                if (!string.IsNullOrEmpty(c.Value)) campi[c.Key] = c.Value;
                        }
                    file++;
                }
                catch (Exception e)
                {
                    _log.LogError($"  dialoghi/{Path.GetFileName(percorso)}: {e.Message}");
                }
            }

            int righe = 0;
            foreach (var c in _conversazioni.Values) righe += c.Count;
            _log.LogInfo($"Dialoghi: {righe} righe tradotte da {file} file "
                         + $"({_attori.Count} attori, {_oggetti.Count} oggetti)");
        }

        // ------------------------------------------------------------------
        // Applicazione al database
        // ------------------------------------------------------------------

        internal void Applica(DialogueDatabase db)
        {
            if (db == null || Applicato) return;
            Applicato = true;

            int voci = 0, tradotte = 0, ereditate = 0;

            foreach (var conv in db.conversations)
            {
                _conversazioni.TryGetValue(conv.id.ToString(), out var nostre);

                foreach (var voce in conv.dialogueEntries)
                {
                    voci++;
                    string testo = null;
                    if (nostre != null && nostre.TryGetValue(voce.id.ToString(), out var tradotto))
                        testo = tradotto;

                    if (testo != null) tradotte++;
                    else
                    {
                        // Mai lasciare il campo vuoto: senza `it` la battuta diventa
                        // muta, perche' il ripiego «Dialogue Text» qui non ha testo.
                        testo = Field.LookupValue(voce.fields, CodiceModello);
                        if (string.IsNullOrEmpty(testo)) continue;   // voce senza testo, e' logica
                        ereditate++;
                    }

                    Field.SetValue(voce.fields, CampoTesto, testo, FieldType.Text);
                }
            }

            foreach (var attore in db.actors)
            {
                string nome = _attori.TryGetValue(attore.id.ToString(), out var n)
                    ? n
                    : Field.LookupValue(attore.fields, "Name " + CodiceModello);
                if (string.IsNullOrEmpty(nome)) nome = Field.LookupValue(attore.fields, "Name");
                if (!string.IsNullOrEmpty(nome))
                    Field.SetValue(attore.fields, "Name " + CampoTesto, nome, FieldType.Text);
            }

            foreach (var oggetto in db.items)
            {
                _oggetti.TryGetValue(oggetto.id.ToString(), out var nostri);
                foreach (string campo in new[] { "Description", "Success Description", "Failure Description" })
                {
                    string testo = null;
                    if (nostri != null && nostri.TryGetValue(campo, out var tradotto)) testo = tradotto;
                    if (string.IsNullOrEmpty(testo))
                        testo = Field.LookupValue(oggetto.fields, campo + " " + CodiceModello);
                    if (string.IsNullOrEmpty(testo))
                        testo = Field.LookupValue(oggetto.fields, campo);
                    if (!string.IsNullOrEmpty(testo))
                        Field.SetValue(oggetto.fields, campo + " " + CampoTesto, testo, FieldType.Text);
                }
            }

            VociTradotte = tradotte;
            _log.LogInfo($"[dialoghi] database '{db.name}': {voci} voci, "
                         + $"{tradotte} in italiano, {ereditate} ancora in inglese");

            Rilettura(db);
        }

        /// <summary>
        /// Rilegge qualche battuta **come la legge il gioco** e la scrive nel log.
        ///
        /// Serve a distinguere «ho scritto i campi» da «il gioco li trova»: se qui
        /// esce vuoto, in partita le battute sarebbero mute, ed e' meglio saperlo dal
        /// log che scoprirlo giocando. Usa la stessa proprieta' che usa il gioco,
        /// <c>currentDialogueText</c>, che passa da <c>GetCurrentDialogueTextField()</c>.
        /// </summary>
        private void Rilettura(DialogueDatabase db)
        {
            string lingua = Localization.language;
            _log.LogInfo($"[dialoghi] lingua del Dialogue System: '{lingua}'");

            int mostrate = 0, vuote = 0, controllate = 0;
            foreach (var conv in db.conversations)
            {
                foreach (var voce in conv.dialogueEntries)
                {
                    if (string.IsNullOrEmpty(Field.LookupValue(voce.fields, CodiceModello))) continue;
                    controllate++;

                    string letto = voce.currentDialogueText;
                    if (string.IsNullOrEmpty(letto)) vuote++;
                    else if (mostrate < 3)
                    {
                        mostrate++;
                        string breve = letto.Length > 70 ? letto.Substring(0, 70) + "…" : letto;
                        _log.LogInfo($"[dialoghi]   esempio: \"{breve}\"");
                    }
                    if (controllate >= 400) break;
                }
                if (controllate >= 400) break;
            }

            if (vuote > 0)
                _log.LogError($"[dialoghi] ATTENZIONE: {vuote} battute su {controllate} risultano vuote: "
                              + "in partita sarebbero mute");
            else
                _log.LogInfo($"[dialoghi] rilettura: {controllate} battute controllate, nessuna vuota");
        }
    }
}
