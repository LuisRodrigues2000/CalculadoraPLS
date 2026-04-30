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
        public const int CapacidadeBkChicken = 24;
        public const int CapacidadeChickenJr = 8;

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
        ///
        /// Fluxo:
        ///   1. Unidades Totais = (recipientes × capacidade) + unidadesAvulsas
        ///   2. Novas Unidades  = Unidades Totais × (1 + diferença% / 100)  → Ceiling
        ///   3. Novos Recipientes = Novas Unidades / capacidade              → Ceiling
        ///   4. Unidades Restantes = Novas Unidades - (Novos Recipientes × capacidade)
        ///
        ///   Obs: se Unidades Totais = 0, o resultado também é zerado.
        /// </summary>
        public static ResultadoCarne CalcularCarne(
            string nome,
            int capacidade,
            int recipientesInformados,
            int unidadesAvulsas,
            double diferencaPercentual)
        {
            // Arredonda o percentual para inteiro antes de aplicar (regra da apostila)
            double diferencaArredondada = Math.Round(
                diferencaPercentual, MidpointRounding.AwayFromZero);

            // 1. Total de unidades base
            int unidadesTotaisBase = (recipientesInformados * capacidade) + unidadesAvulsas;

            // 2. Aplica o percentual arredondado
            double unidadesExatas = unidadesTotaisBase * (1.0 + (diferencaArredondada / 100.0));
            unidadesExatas = Math.Max(0, unidadesExatas);

            // 3. ✅ Arredondamento matemático convencional (não mais Ceiling)
            //    < 0,5 → arredonda pra baixo | >= 0,5 → arredonda pra cima
            int unidadesFinais = (int)Math.Round(
                unidadesExatas, MidpointRounding.AwayFromZero);

            bool foiArredondado = unidadesExatas != Math.Floor(unidadesExatas);

            return new ResultadoCarne
            {
                Nome = nome,
                Capacidade = capacidade,
                RecipientesInformados = recipientesInformados,
                UnidadesAvulsasInformadas = unidadesAvulsas,
                UnidadesTotaisBase = unidadesTotaisBase,
                UnidadesExatas = unidadesExatas,
                UnidadesFinais = unidadesFinais,
                FoiArredondado = foiArredondado
            };
        }
    }
}
