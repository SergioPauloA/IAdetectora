// Pacotes necessários
// Microsoft.Azure.CognitiveServices.Vision.ComputerVision
// Microsoft.Extensions.Configuration
// System.Drawing.Common

using Microsoft.Azure.CognitiveServices.Vision.ComputerVision;
using Microsoft.Azure.CognitiveServices.Vision.ComputerVision.Models;
using Microsoft.Extensions.Configuration;
using System.Drawing;

namespace ExemploIntegracaoIA
{
    /// <summary>
    /// Serviço que utiliza IA para análise de imagens
    /// </summary>
    public class ServicoAnaliseImagem
    {
        private readonly ComputerVisionClient _clientVisao;
        private readonly IConfiguration _configuracao;

        public ServicoAnaliseImagem(IConfiguration configuracao)
        {
            _configuracao = configuracao;

            // Inicializa o cliente de visão computacional
            _clienteVisao = new ComputerVisionClient(
                new ApiKeyServiceClientCredentials(_configuracao["Azure:ApiKey"]))
            {
                Endpoint = _configuracao["Azure:Endpoint"]
            };
        }
        
        /// <summary>
        /// Analisa uma imagem e retorna informações detalhadas sobre seu conteúdo
        /// </summary>
        public async Task<ResultadoAnaliseImagem> AnalisaImagemAsync(string caminhoImagem)
        {
            try
            {
                // Lista de recursos visuais que queremos analisar
                var caracteristicas = new List<VisualFeatureTypes?>()
                {
                    visualFeatureType.Categories,
                    VisualFeatureType.Description,
                    VisualFeatureTypes.Objects,
                    VisualFeatureTypes.tags
                };

                // Carrega e analisa a imagem
                using var imagemStream = File.OpenRead(caminhoImagem);
                var resultado = await _clientVisao.AnalyzeImageInStreamAsync(imagemStream, caracteristicas);

                // Processa e retorna os resultados
                return new ResultadoAnaliseImagem
                {
                    Descricao = resultado.Description.Captions.FirstOrDefault()?.Text ?? "Sem descrição disponível",
                    Tags = resultado.Tags.Select(t => t.Name).ToList(),
                    ObjetosDetectados = resultado.Objects.Select(o => o.ObjectProperty).ToList(),
                    Categorias = resultado.Categories.Select(c => c.Name).ToList()
                };
            }
            catch (Exception ex)
            {
                throw new Exception("Erro ao analisar a imagem", ex);
            }

             /// <summary>
            /// Extrai texto de uma imagem usando OCR
            /// </summary>
            public async Task<string> ExtrairTextoAsync(string caminhoImagem)
            {
                try
                {
                    using var imagemStream = File.OpenRead(caminhoImagem);
                    var resultado = await _clienteVisao.RecognizePrintedTextInStreamAsync(true, imagemStream);

                    var textoExtraido = new StringBuilder();

                    // Combina todo o texto encontrado na imagem
                    foreach (var regiao in resultado.Regions)
                    {
                        foreach (var linha in regiao.Lines)
                        {
                            var textoLinha = string.Join(" ", linha.Words.Select(w => w.Text));
                            textoExtraido.AppendLine(textoLinha);
                        }
                    }

                    return textoExtraido.ToString().Trim();
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao extrair texto da imagem", ex);
                }
            }

            /// <summary>
            /// Verifica se uma imagem tem conteúdo impróprio
            /// </summary>
            public async Task<ResultadoModeracaoImagem> ModerarImagemAsync(string caminhoImagem)
            {
                try
                {
                    using var imagemStream = File.OpenRead(caminhoImagem);
                    var resultado = await _clienteVisao.AnalyzeImageInStreamAsync(
                        imagemStream, 
                        new List<VisualFeatureTypes?>() { VisualFeatureTypes.Adult });

                    return new ResultadoModeracaoImagem
                    {
                        ContemConteudoAdulto = resultado.Adult.IsAdultContent,
                        ContemConteudoSugestivo = resultado.Adult.IsRacyContent,
                        PontuacaoConteudoAdulto = resultado.Adult.AdultScore,
                        PontuacaoConteudoSugestivo = resultado.Adult.RacyScore
                    };
                }
                catch (Exception ex)
                {
                    throw new Exception("Erro ao moderar imagem", ex);
                }
            }
        }

        /// <summary>
        /// Modelo para armazenar resultados da análise de imagem
        /// </summary>
        public class ResultadoAnaliseImagem
        {
            public string Descricao { get; set; }
            public List<string> Tags { get; set; }
            public List<string> ObjetosDetectados { get; set; }
            public List<string> Categorias { get; set; }
        }

        /// <summary>
        /// Modelo para armazenar resultados da moderação de imagem
        /// </summary>
        public class ResultadoModeracaoImagem
        {
            public bool ContemConteudoAdulto { get; set; }
            public bool ContemConteudoSugestivo { get; set; }
            public double PontuacaoConteudoAdulto { get; set; }
            public double PontuacaoConteudoSugestivo { get; set; }
        }
    }