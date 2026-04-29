using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CalculadoraPLS.Models;

namespace CalculadoraPLS.ViewModels
{
    /// <summary>
    /// ViewModel principal da calculadora PLS.
    /// Implementa INotifyPropertyChanged para atualizar a tela automaticamente.
    /// </summary>
    public class PlsViewModel : INotifyPropertyChanged
    {
        // ─── Cultura pt-BR para parsing com vírgula decimal ──────────────────
        private static readonly CultureInfo _ptBR = new("pt-BR");

        // ─── Campos privados ─────────────────────────────────────────────────
        private string _vendaEstimada = string.Empty;
        private string _vendaReal = string.Empty;
        private string _vendaEstimadaFormatada = string.Empty;
        private string _vendaRealFormatada = string.Empty;
        private double _diferencaReal;
        private bool _resultadoVisivel;
        private string _corDiferenca = "#3A3A6A";   // neutro → azul escuro
        private string _corDiferencaFundo = "#0F0F2A";   // neutro → fundo do card
        private string _labelTendencia = "● IGUAL";
        private string _diferencaFormatada = "0,00%";

        // ─── Propriedades de entrada ─────────────────────────────────────────

        /// <summary>
        /// Valor bruto da venda estimada enviado pelo code-behind após
        /// a máscara monetária (formato "369.17" em invariant culture).
        /// </summary>
        public string VendaEstimada
        {
            get => _vendaEstimada;
            set
            {
                _vendaEstimada = value;
                OnPropertyChanged();
                ((Command)CalcularCommand).ChangeCanExecute();
            }
        }

        /// <summary>
        /// Valor bruto da venda real enviado pelo code-behind após
        /// a máscara monetária (formato "369.17" em invariant culture).
        /// </summary>
        public string VendaReal
        {
            get => _vendaReal;
            set
            {
                _vendaReal = value;
                OnPropertyChanged();
                ((Command)CalcularCommand).ChangeCanExecute();
            }
        }

        /// <summary>
        /// Texto já formatado em R$ exibido pelo Entry de Venda Estimada.
        /// Atualizado pelo code-behind via máscara — binding somente leitura.
        /// </summary>
        public string VendaEstimadaFormatada
        {
            get => _vendaEstimadaFormatada;
            set { _vendaEstimadaFormatada = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Texto já formatado em R$ exibido pelo Entry de Venda Real.
        /// Atualizado pelo code-behind via máscara — binding somente leitura.
        /// </summary>
        public string VendaRealFormatada
        {
            get => _vendaRealFormatada;
            set { _vendaRealFormatada = value; OnPropertyChanged(); }
        }

        // ─── Propriedades de resultado ────────────────────────────────────────

        /// <summary>
        /// Valor numérico puro da diferença percentual.
        /// Usado pelo code-behind para o contador animado (Ponto 8).
        /// </summary>
        public double DiferencaReal
        {
            get => _diferencaReal;
            private set
            {
                _diferencaReal = value;
                OnPropertyChanged();
                AtualizarCorELabel();
            }
        }

        /// <summary>
        /// Texto formatado da diferença. Ex: "+61,09%" ou "-5,30%".
        /// Atualizado por AtualizarCorELabel().
        /// </summary>
        public string DiferencaFormatada
        {
            get => _diferencaFormatada;
            private set { _diferencaFormatada = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Cor do texto de diferença.
        /// Acima  → azul  (#0008FF)
        /// Abaixo → vermelho (#F44336)
        /// Igual  → cinza-azulado (#3A3A6A)
        /// </summary>
        public string CorDiferenca
        {
            get => _corDiferenca;
            private set { _corDiferenca = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Cor de fundo do pill de tendência (versão escurecida da cor principal).
        /// </summary>
        public string CorDiferencaFundo
        {
            get => _corDiferencaFundo;
            private set { _corDiferencaFundo = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Texto do pill de tendência. Ex: "▲ ACIMA", "▼ ABAIXO", "● IGUAL".
        /// </summary>
        public string LabelTendencia
        {
            get => _labelTendencia;
            private set { _labelTendencia = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Controla a visibilidade do painel de resultados.
        /// </summary>
        public bool ResultadoVisivel
        {
            get => _resultadoVisivel;
            set { _resultadoVisivel = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Lista de resultados por tipo de carne.
        /// </summary>
        public ObservableCollection<ResultadoCarne> Resultados { get; } = new();

        // ─── Comandos ─────────────────────────────────────────────────────────

        public ICommand CalcularCommand { get; }
        public ICommand LimparCommand { get; }

        // ─── Construtor ───────────────────────────────────────────────────────

        public PlsViewModel()
        {
            CalcularCommand = new Command(Calcular, PodeCalcular);
            LimparCommand = new Command(Limpar);
        }

        // ─── Lógica principal ─────────────────────────────────────────────────

        /// <summary>
        /// Habilita o botão Calcular somente se os dois campos forem válidos
        /// e a venda estimada for maior que zero.
        /// </summary>
        private bool PodeCalcular()
        {
            return TryParseValor(VendaEstimada, out double est) &&
                   TryParseValor(VendaReal, out _) &&
                   est > 0;
        }

        /// <summary>
        /// Executa o cálculo principal do PLS.
        /// </summary>
        private void Calcular()
        {
            if (!TryParseValor(VendaEstimada, out double estimada) ||
                !TryParseValor(VendaReal, out double real))
                return;

            // Calcula e dispara todas as atualizações de cor/label/formatação
            DiferencaReal = PlsModel.CalcularDiferencaPercentual(estimada, real);

            // Reconstrói os cards de resultado
            Resultados.Clear();
            Resultados.Add(PlsModel.CalcularCarne("HB", PlsModel.CapacidadeHB, DiferencaReal));
            Resultados.Add(PlsModel.CalcularCarne("Whopper", PlsModel.CapacidadeWhopper, DiferencaReal));
            Resultados.Add(PlsModel.CalcularCarne("Rebel", PlsModel.CapacidadeRebel, DiferencaReal));

            ResultadoVisivel = true;
        }

        /// <summary>
        /// Reseta todos os campos para o estado inicial.
        /// </summary>
        private void Limpar()
        {
            VendaEstimada = string.Empty;
            VendaReal = string.Empty;
            VendaEstimadaFormatada = string.Empty;
            VendaRealFormatada = string.Empty;
            DiferencaReal = 0;
            ResultadoVisivel = false;
            Resultados.Clear();
        }

        /// <summary>
        /// Atualiza cor, fundo e label de tendência conforme o sinal da diferença.
        ///
        /// Paleta azul/branco:
        ///   Acima  → texto #0008FF  | fundo #08082A  | pill "▲ ACIMA"
        ///   Abaixo → texto #F44336  | fundo #2A0D0D  | pill "▼ ABAIXO"
        ///   Igual  → texto #3A3A6A  | fundo #0F0F2A  | pill "● IGUAL"
        /// </summary>
        private void AtualizarCorELabel()
        {
            var sinal = _diferencaReal >= 0 ? "+" : "";
            DiferencaFormatada = $"{sinal}{_diferencaReal:F2}%";

            if (_diferencaReal > 0)
            {
                CorDiferenca = "#0008FF";   // Azul elétrico → acima
                CorDiferencaFundo = "#EEF0FF";   // Azul bem claro para fundo branco
                LabelTendencia = "▲ ACIMA";
            }
            else if (_diferencaReal < 0)
            {
                CorDiferenca = "#D32F2F";   // Vermelho escuro → abaixo
                CorDiferencaFundo = "#FFEEEE";   // Vermelho bem claro para fundo branco
                LabelTendencia = "▼ ABAIXO";
            }
            else
            {
                CorDiferenca = "#AAAACC";   // Cinza-azulado → igual
                CorDiferencaFundo = "#F5F5FA";   // Cinza claríssimo
                LabelTendencia = "● IGUAL";
            }
        }
        // ─── Helper de parsing ────────────────────────────────────────────────

        /// <summary>
        /// Faz parse de string decimal aceitando tanto ponto (invariant)
        /// quanto vírgula (pt-BR) como separador.
        /// </summary>
        private static bool TryParseValor(string texto, out double valor)
        {
            if (string.IsNullOrWhiteSpace(texto))
            {
                valor = 0;
                return false;
            }

            // Tenta invariant (ponto decimal — formato enviado pelo code-behind)
            if (double.TryParse(texto, NumberStyles.Any,
                                CultureInfo.InvariantCulture, out valor))
                return true;

            // Fallback: pt-BR (vírgula decimal)
            if (double.TryParse(texto, NumberStyles.Any, _ptBR, out valor))
                return true;

            valor = 0;
            return false;
        }

        // ─── INotifyPropertyChanged ───────────────────────────────────────────

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
