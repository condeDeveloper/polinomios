using System.Numerics;

namespace Conde.Polinomios;

/// <summary>
/// As MEDIDAS: as tabelas que o medidor imprime e que os testes conferem.
///
/// Elas ficam aqui, e nao no medidor, para o teste poder afirmar sobre o MESMO
/// numero que aparece no texto. Tabela de README que ninguem testa envelhece
/// errado em silencio.
///
/// Nenhuma delas usa relogio. Todas sao contagem de multiplicacoes de
/// coeficiente, de digitos ou de respostas erradas.
/// </summary>
public static class Medidas
{
    public sealed record LinhaCusto(int Grau, long Escolar, long Karatsuba, long Transformada, double Razao);

    /// <summary>
    /// O CUSTO das tres multiplicacoes, em multiplicacoes de coeficiente.
    /// </summary>
    public static List<LinhaCusto> Custos()
    {
        var linhas = new List<LinhaCusto>();
        var sorteio = new Sorteio(7);

        foreach (var grau in new[] { 15, 31, 63, 127, 255, 511, 1023 })
        {
            var a = Polinomio.Sortear(grau, sorteio);
            var b = Polinomio.Sortear(grau, sorteio);

            var escolar = Multiplicacao.Escolar(a, b).Multiplicacoes;
            var karatsuba = Multiplicacao.Karatsuba(a, b).Multiplicacoes;
            var transformada = Ntt.Multiplicacoes(a.Termos, b.Termos);

            linhas.Add(new LinhaCusto(grau, escolar, karatsuba, transformada, (double)escolar / karatsuba));
        }

        return linhas;
    }

    public sealed record LinhaJuiz(int Grau, int Casos, bool EscolarBate, bool KaratsubaBate, bool TransformadaBate);

    /// <summary>
    /// O JUIZ: os tres metodos avaliados nos mesmos pontos, em muitos pares de
    /// polinomios sorteados.
    ///
    /// Nao se compara vetor de coeficientes: compara-se o POLINOMIO, avaliando nos
    /// pontos. Concordar em grau+1 pontos distintos nao e evidencia de igualdade,
    /// e a igualdade.
    /// </summary>
    public static List<LinhaJuiz> Juiz(int casos = 100)
    {
        var linhas = new List<LinhaJuiz>();
        var sorteio = new Sorteio(11);

        foreach (var grau in new[] { 7, 15, 31, 63 })
        {
            var escolar = true;
            var karatsuba = true;
            var transformada = true;

            for (var i = 0; i < casos; i++)
            {
                var a = Polinomio.Sortear(grau, sorteio, limite: 50);
                var b = Polinomio.Sortear(grau, sorteio, limite: 50);

                var certo = Multiplicacao.Escolar(a, b).Produto;

                if (!Identidade.SaoIguais(certo, Multiplicacao.Escolar(a, b).Produto)) escolar = false;
                if (!Identidade.SaoIguais(certo, Multiplicacao.Karatsuba(a, b).Produto)) karatsuba = false;
                if (Ntt.CabeNoPrimo(a, b) && !Identidade.SaoIguais(certo, Ntt.Multiplicar(a, b))) transformada = false;
            }

            linhas.Add(new LinhaJuiz(grau, casos, escolar, karatsuba, transformada));
        }

        return linhas;
    }

    public sealed record LinhaFlutuante(int Grau, int Coeficiente, string MaiorPossivel, double MaiorDesvio, bool Acertou);

    /// <summary>
    /// A TRANSFORMADA COM VIRGULA contra a exata: onde o arredondamento comeca a
    /// errar.
    ///
    /// A coluna do desvio e a que avisa antes: enquanto ele fica bem abaixo de
    /// meio, o arredondamento acerta.
    /// </summary>
    public static List<LinhaFlutuante> Flutuantes()
    {
        var linhas = new List<LinhaFlutuante>();
        var sorteio = new Sorteio(3);

        foreach (var (grau, coeficiente) in new[] { (63, 1_000), (255, 100_000), (511, 10_000_000), (1023, 1_000_000_000) })
        {
            var a = Polinomio.Sortear(grau, sorteio, coeficiente);
            var b = Polinomio.Sortear(grau, sorteio, coeficiente);

            var certo = Multiplicacao.Escolar(a, b).Produto;
            var comVirgula = Flutuante.Multiplicar(a, b);

            linhas.Add(new LinhaFlutuante(
                grau,
                coeficiente,
                Flutuante.MaiorCoeficientePossivel(a, b).ToString(),
                Flutuante.MaiorDesvio(a, b),
                Identidade.SaoIguais(certo, comVirgula)));
        }

        return linhas;
    }

    public sealed record LinhaMaximo(int Grau, int GrauDoFator, int PassosIngenuo, int DigitosIngenuo, int PassosPrimitivo, int DigitosPrimitivo);

    /// <summary>
    /// O MAXIMO DIVISOR COMUM: os coeficientes explodindo, e a linha a mais que
    /// resolve.
    /// </summary>
    public static List<LinhaMaximo> Maximos()
    {
        var linhas = new List<LinhaMaximo>();
        var sorteio = new Sorteio(5);

        foreach (var (grau, grauDoFator) in new[] { (6, 2), (8, 3), (10, 3), (12, 4) })
        {
            var (a, b, _) = Divisao.ComFatorComum(grau, grauDoFator, sorteio);

            var ingenuo = Divisao.MaximoIngenuo(a, b);
            var primitivo = Divisao.MaximoPrimitivo(a, b);

            linhas.Add(new LinhaMaximo(grau, grauDoFator, ingenuo.Passos, ingenuo.MaiorDigitos, primitivo.Passos, primitivo.MaiorDigitos));
        }

        return linhas;
    }

    public sealed record LinhaSorteio(int Grau, int Faixa, int Tentativas, int PrimeiroPontoPegou, double ChanceDeFalhar);

    /// <summary>
    /// O TEOREMA FUNCIONANDO: dois polinomios diferentes avaliados num ponto
    /// sorteado.
    ///
    /// A chance de o ponto cair numa raiz da diferenca e no maximo grau sobre
    /// faixa, e a medida bate com a conta.
    /// </summary>
    public static List<LinhaSorteio> Sorteios(int tentativas = 10_000)
    {
        var linhas = new List<LinhaSorteio>();
        var sorteio = new Sorteio(13);

        foreach (var (grau, faixa) in new[] { (10, 20), (10, 100), (50, 100), (50, 10_000) })
        {
            // a diferenca entre os dois e um polinomio com EXATAMENTE grau
            // raizes inteiras, de 1 ate o grau: o pior caso possivel para o
            // teste sorteado, e e sobre o pior caso que o teorema fala
            var a = Polinomio.Sortear(grau, sorteio, limite: 20);
            var diferenca = Polinomio.Um;

            for (var raiz = 1; raiz <= grau; raiz++)
                diferenca = Multiplicacao.Escolar(diferenca, Polinomio.De(-raiz, 1)).Produto;

            var b = a + diferenca;

            linhas.Add(new LinhaSorteio(
                grau,
                faixa,
                tentativas,
                Identidade.PrimeiroPontoJaPegou(a, b, sorteio, tentativas, faixa),
                Identidade.ChanceDeErrar(grau, 1, faixa)));
        }

        return linhas;
    }

    public sealed record LinhaCorte(int Corte, long Multiplicacoes);

    /// <summary>
    /// O CORTE do Karatsuba: a partir de que tamanho vale a pena recursar.
    ///
    /// Contando so multiplicacoes, o corte menor sempre ganha, e e por isso que a
    /// contagem de multiplicacoes NAO decide a escolha sozinha: as somas, as
    /// copias e as chamadas que ela nao conta sao o motivo de o corte de verdade
    /// ficar em algumas dezenas.
    /// </summary>
    public static List<LinhaCorte> Cortes(int grau = 1023)
    {
        var sorteio = new Sorteio(7);
        var a = Polinomio.Sortear(grau, sorteio);
        var b = Polinomio.Sortear(grau, sorteio);

        return [.. new[] { 1, 2, 4, 8, 16, 32, 64, 128 }
            .Select(corte => new LinhaCorte(corte, Multiplicacao.Karatsuba(a, b, corte).Multiplicacoes))];
    }
}
