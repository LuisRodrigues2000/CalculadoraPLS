namespace CalculadoraPLS.Models
{
    public class ResultadoCarne
    {
        // ─── Identificação ───────────────────────────────────────────────────
        public string Nome { get; set; } = string.Empty;
        public int Capacidade { get; set; }

        // ─── Input do usuário ────────────────────────────────────────────────
        public int RecipientesInformados { get; set; }
        public int UnidadesAvulsasInformadas { get; set; }

        // ─── Resultado do cálculo ────────────────────────────────────────────

        /// <summary>
        /// Total de unidades base: (RecipientesInformados × Capacidade) + UnidadesAvulsasInformadas
        /// </summary>
        public int UnidadesTotaisBase { get; set; }

        /// <summary>Valor calculado ANTES do arredondamento. Ex: 12,72</summary>
        public double UnidadesExatas { get; set; }

        /// <summary>Valor APÓS Ceiling. Ex: 13</summary>
        public int UnidadesFinais { get; set; }

        /// <summary>
        /// Recipientes COMPLETOS que cabem nas unidades finais.
        /// Ex: 13 / 12 = 1 recipiente completo.
        /// Se UnidadesFinais menor que Capacidade → 0 recipientes.
        /// </summary>
        public int RecipientesFinal => UnidadesFinais / Capacidade;

        /// <summary>
        /// Unidades que sobram após os recipientes completos.
        /// Ex: 13 % 12 = 1 unidade restante.
        /// Se resultado cabe em menos de 1 recipiente → todas as unidades aparecem aqui.
        /// </summary>
        public int UnidadesRestantesFinal => UnidadesFinais % Capacidade;

        /// <summary>Indica se o valor exato foi arredondado para cima.</summary>
        public bool FoiArredondado { get; set; }

        /// <summary>
        /// Mensagem descritiva do arredondamento.
        /// Ex: "12,72 → 13 unidades"
        /// </summary>
        public string MensagemArredondamento =>
            FoiArredondado
                ? $"{UnidadesExatas:F2} → {UnidadesFinais} unidades"
                : string.Empty;
    }
}
