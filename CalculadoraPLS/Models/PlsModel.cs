namespace CalculadoraPLS.Models
{
    /// <summary>
    /// Model principal da calculadora PLS.
    /// Contém as constantes de capacidade e a lógica de cálculo.
    /// </summary>
    public static class PlsModel
    {
        // ─── Capacidade fixa por recipiente ─────────────────────────────────
        public const int CapacidadeHB = 12;
        public const int CapacidadeWhopper = 9;
        public const int CapacidadeRebel = 8;

        /// <summary>
        /// Calcula a diferença percentual entre venda real e estimada.
        /// Fórmula: ((Real - Estimada) / Estimada) × 100
        /// </summary>
        public static double CalcularDiferencaPercentual(double estimada, double real)
        {
            if (estimada == 0) return 0;
            return ((real - estimada) / estimada) * 100;
        }

        /// <summary>
        /// Calcula o resultado para um tipo de carne.
        /// Fórmula: Unidades = Capacidade × (1 + Diferença% / 100)
        /// Arredondamento: para baixo (Floor).
        /// Recipientes e UnidadesRestantes são calculados automaticamente
        /// via propriedades em ResultadoCarne.
        /// </summary>
        public static ResultadoCarne CalcularCarne(
            string nome,
            int capacidade,
            double diferencaPercentual)
        {
            // Aplica a diferença percentual sobre a capacidade do recipiente
            double unidadesExatas = capacidade * (1 + (diferencaPercentual / 100));

            // Garante que nunca seja negativo
            unidadesExatas = Math.Max(0, unidadesExatas);

            // Arredonda para baixo (Floor)
            int unidadesArredondadas = (int)Math.Floor(unidadesExatas);

            // Verifica se houve arredondamento
            bool foiArredondado = unidadesExatas != unidadesArredondadas;

            return new ResultadoCarne
            {
                Nome = nome,
                Capacidade = capacidade,
                UnidadesExatas = unidadesExatas,
                UnidadesArredondadas = unidadesArredondadas,
                FoiArredondado = foiArredondado
                // ✅ Recipientes e UnidadesRestantes são propriedades
                //    calculadas automaticamente em ResultadoCarne
            };
        }
    }
}
