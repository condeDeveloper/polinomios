using Conde.Polinomios;
using Xunit;

namespace Conde.Polinomios.Testes;

/// <summary>
/// Os testes das TABELAS que o medidor imprime.
///
/// Eles existem porque numero em README que ninguem confere envelhece errado em
/// silencio: alguem troca uma constante, a tabela muda e o texto continua dizendo
/// a frase antiga. Aqui o teste afirma sobre o mesmo objeto que vira linha
/// impressa.
/// </summary>
public class MedidasTestes
{
    /// <summary>
    /// A linha principal: os tres metodos dao o mesmo polinomio, sempre.
    /// </summary>
    [Fact]
    public void OsTresMetodosConcordamEmTodosOsGraus()
    {
        var linhas = Medidas.Juiz();

        Assert.Equal(4, linhas.Count);
        Assert.All(linhas, l => Assert.True(l.EscolarBate));
        Assert.All(linhas, l => Assert.True(l.KaratsubaBate));
        Assert.All(linhas, l => Assert.True(l.TransformadaBate));
    }

    /// <summary>
    /// A frase do texto: com grau pequeno o Karatsuba empata com a escolar, e com
    /// grau mil ele gasta um quarto.
    /// </summary>
    [Fact]
    public void OKaratsubaSoGanhaDepoisDeUmTamanho()
    {
        var linhas = Medidas.Custos();

        Assert.Equal(7, linhas.Count);

        var pequeno = linhas.First(l => l.Grau == 15);
        var grande = linhas.First(l => l.Grau == 1023);

        Assert.Equal(1.0, pequeno.Razao, 6);
        Assert.InRange(grande.Razao, 3.5, 5.0);

        // e a razao so cresce
        for (var i = 1; i < linhas.Count; i++) Assert.True(linhas[i].Razao >= linhas[i - 1].Razao);
    }

    /// <summary>
    /// E a transformada ganha das duas a partir de um grau baixo.
    /// </summary>
    [Fact]
    public void ATransformadaGanhaDasDuas()
    {
        foreach (var l in Medidas.Custos().Where(l => l.Grau >= 63))
        {
            Assert.True(l.Transformada < l.Escolar, $"grau {l.Grau}");
            Assert.True(l.Transformada < l.Karatsuba, $"grau {l.Grau}");
        }
    }

    /// <summary>
    /// O CORTE do Karatsuba, contando so multiplicacoes, SEMPRE prefere o corte
    /// menor. E isso e exatamente o motivo de a contagem de multiplicacoes nao
    /// decidir a escolha sozinha: as somas, as copias e as chamadas que ela nao
    /// conta sao o que poe o corte de verdade em algumas dezenas.
    /// </summary>
    [Fact]
    public void AContagemDeMultiplicacoesSempreQuerOCorteMenor()
    {
        var linhas = Medidas.Cortes();

        Assert.Equal(8, linhas.Count);

        for (var i = 1; i < linhas.Count; i++)
            Assert.True(linhas[i].Multiplicacoes > linhas[i - 1].Multiplicacoes,
                $"corte {linhas[i].Corte} nao gastou mais que o corte {linhas[i - 1].Corte}");
    }

    /// <summary>
    /// A frase do texto: a transformada com virgula acerta ate um tamanho e erra
    /// depois dele.
    /// </summary>
    [Fact]
    public void AComVirgulaAcertaAteUmTamanhoEErraDepois()
    {
        var linhas = Medidas.Flutuantes();

        Assert.Equal(4, linhas.Count);
        Assert.True(linhas[0].Acertou);
        Assert.True(linhas[1].Acertou);
        Assert.False(linhas[2].Acertou);
        Assert.False(linhas[3].Acertou);
    }

    /// <summary>
    /// E o aviso do desvio para de funcionar na ultima linha, que e a pior de
    /// todas: acima de 2^53 todo numero de precisao dupla ja e inteiro, entao o
    /// desvio da zero.
    /// </summary>
    [Fact]
    public void OAvisoDoDesvioSomeNaPiorLinha()
    {
        var linhas = Medidas.Flutuantes();

        Assert.True(linhas[0].MaiorDesvio < 1e-6);
        Assert.True(linhas[2].MaiorDesvio > 0.1);
        Assert.Equal(0.0, linhas[3].MaiorDesvio, 12);
        Assert.False(linhas[3].Acertou);
    }

    /// <summary>
    /// A frase do texto: os coeficientes explodem no Euclides direto, e tirar o
    /// conteudo resolve.
    /// </summary>
    [Fact]
    public void OsCoeficientesExplodemETirarOConteudoResolve()
    {
        var linhas = Medidas.Maximos();

        Assert.Equal(4, linhas.Count);

        foreach (var l in linhas)
        {
            Assert.True(l.DigitosIngenuo > 100, $"grau {l.Grau}: so {l.DigitosIngenuo} digitos");
            Assert.True(l.DigitosPrimitivo < 50, $"grau {l.Grau}: {l.DigitosPrimitivo} digitos mesmo com o conteudo tirado");
            Assert.True(l.PassosIngenuo < 20);
        }

        // e o caminho fica pior conforme o grau cresce
        Assert.True(linhas[^1].DigitosIngenuo > linhas[0].DigitosIngenuo * 5);
    }

    /// <summary>
    /// A frase do texto: a chance medida de o ponto sorteado falhar bate com a
    /// conta do teorema.
    /// </summary>
    [Fact]
    public void AChanceMedidaBateComAContaDoTeorema()
    {
        var linhas = Medidas.Sorteios();

        Assert.Equal(4, linhas.Count);

        foreach (var l in linhas)
        {
            var falhou = 1 - (double)l.PrimeiroPontoPegou / l.Tentativas;

            Assert.True(falhou <= l.ChanceDeFalhar + 0.02,
                $"grau {l.Grau}, faixa {l.Faixa}: falhou {falhou:P2}, o teorema permite {l.ChanceDeFalhar:P2}");
        }

        // e com a faixa grande ela quase nunca falha
        var comFaixaGrande = linhas.Single(l => l.Faixa == 10_000);
        Assert.True((double)comFaixaGrande.PrimeiroPontoPegou / comFaixaGrande.Tentativas > 0.99);
    }
}
