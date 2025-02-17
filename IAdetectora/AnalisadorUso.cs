// Em seu Controller ou serviço
public class ImagemController : ControllerBase
{
    private readonly ServicoAnaliseImagem _servicoImagem;

    public ImagemController(ServicoAnaliseImagem servicoImagem)
    {
        _servicoImagem = servicoImagem;
    }

    [HttpPost("analisar")]
    public async Task<IActionResult> AnalisarImagem(IFormFile imagem)
    {
        // Salva a imagem temporariamente
        var caminhoTemp = Path.GetTempFileName();
        using (var stream = new FileStream(caminhoTemp, FileMode.Create))
        {
            await imagem.CopyToAsync(stream);
        }

        // Analisa a imagem
        var resultado = await _servicoImagem.AnalisarImagemAsync(caminhoTemp);
        
        // Limpa o arquivo temporário
        File.Delete(caminhoTemp);

        return Ok(resultado);
    }
}