namespace CalculadoraPLS.Models
{
    /// <summary>
    /// Representa o resultado calculado para um tipo de carne.
    /// </summary>
    public class ResultadoCarne
    {
        /// <summary>Nome do tipo de carne. Ex: "HB", "Whopper", "Rebel"</summary>
        public string Nome { get; set; } = string.Empty;

        /// <summary>Capacidade fixa do recipiente. Ex: 12, 9 ou 8</summary>
        public int Capacidade { get; set; }

        /// <summary>Valor calculado antes do arredondamento. Ex: 19,33</summary>
        public double UnidadesExatas { get; set; }

        /// <summary>Valor após arredondamento para baixo. Ex: 19</summary>
        public int UnidadesArredondadas { get; set; }

        /// <summary>
        /// Quantos recipientes COMPLETOS cabem nas unidades calculadas.
        /// Ex: 19 / 12 = 1 recipiente completo
        /// </summary>
        public int Recipientes => UnidadesArredondadas / Capacidade;

        /// <summary>
        /// Sobra de unidades que NÃO completam um recipiente.
        /// Ex: 19 % 12 = 7 unidades restantes
        /// </summary>
        public int UnidadesRestantes => UnidadesArredondadas % Capacidade;

        /// <summary>Indica se o valor foi arredondado.</summary>
        public bool FoiArredondado { get; set; }

        /// <summary>
        /// Texto descritivo do arredondamento para exibir na tela.
        /// Ex: "Valor original: 19,33 arredondado para 19"
        /// </summary>
        public string MensagemArredondamento =>
            FoiArredondado
                ? $"Valor original: {UnidadesExatas:F2} → arredondado para {UnidadesArredondadas}"
                : string.Empty;
    }
}
