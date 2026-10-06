using Conde.Polinomios;

var pedido = args.Length > 0 ? args[0] : "tudo";
var tudo = pedido == "tudo";

if (tudo || pedido == "juiz") Juiz();
if (tudo || pedido == "custo") Custos();
if (tudo || pedido == "virgula") Flutuantes();
if (tudo || pedido == "maximo") Maximos();

static void Juiz()
{
    Console.WriteLine();
    Console.WriteLine("  O JUIZ: OS TRES METODOS AVALIADOS NOS MESMOS PONTOS");
    Console.WriteLine("  (concordar em grau+1 pontos distintos nao e evidencia: e o teorema)");
    Console.WriteLine();
    Console.WriteLine("  grau   casos   escolar   karatsuba   transformada");

    foreach (var l in Medidas.Juiz())
        Console.WriteLine($"  {l.Grau,4} {l.Casos,7} {Marca(l.EscolarBate),9} {Marca(l.KaratsubaBate),11} {Marca(l.TransformadaBate),14}");

    Console.WriteLine();
    Console.WriteLine("  E O TEOREMA FUNCIONANDO: UM PONTO SORTEADO PEGA A DIFERENCA");
    Console.WriteLine();
    Console.WriteLine("  grau   faixa   tentativas   o primeiro ponto ja pegou   chance de falhar");

    foreach (var l in Medidas.Sorteios())
        Console.WriteLine($"  {l.Grau,4} {l.Faixa,7:N0} {l.Tentativas,12:N0} {(double)l.PrimeiroPontoPegou / l.Tentativas,26:P2} {l.ChanceDeFalhar,18:P2}");

    Console.WriteLine();
}

static void Custos()
{
    Console.WriteLine();
    Console.WriteLine("  MULTIPLICACOES DE COEFICIENTE (nao segundos)");
    Console.WriteLine();
    Console.WriteLine("  grau        escolar     karatsuba   transformada   escolar/karatsuba");

    foreach (var l in Medidas.Custos())
        Console.WriteLine($"  {l.Grau,4} {l.Escolar,14:N0} {l.Karatsuba,13:N0} {l.Transformada,14:N0} {l.Razao,19:F1}x");

    Console.WriteLine();
    Console.WriteLine("  O CORTE DO KARATSUBA (grau 1023)");
    Console.WriteLine();
    Console.WriteLine("  corte   multiplicacoes");

    foreach (var l in Medidas.Cortes())
        Console.WriteLine($"  {l.Corte,5} {l.Multiplicacoes,16:N0}");

    Console.WriteLine();
}

static void Flutuantes()
{
    Console.WriteLine();
    Console.WriteLine("  A TRANSFORMADA COM VIRGULA CONTRA A EXATA");
    Console.WriteLine();
    Console.WriteLine("  grau   coeficiente ate   maior coeficiente possivel   maior desvio   acertou");

    foreach (var l in Medidas.Flutuantes())
        Console.WriteLine($"  {l.Grau,4} {l.Coeficiente,17:N0} {l.MaiorPossivel,28} {l.MaiorDesvio,14:E2} {Marca(l.Acertou),9}");

    Console.WriteLine();
    Console.WriteLine($"  a precisao dupla guarda inteiros exatos ate 2^53 = {Flutuante.LimiteDaPrecisaoDupla:N0}");
    Console.WriteLine($"  o primo da transformada numerica e {Ntt.Primo:N0}, e la nao ha arredondamento nenhum");
    Console.WriteLine();
}

static void Maximos()
{
    Console.WriteLine();
    Console.WriteLine("  O MAXIMO DIVISOR COMUM: OS COEFICIENTES EXPLODINDO");
    Console.WriteLine("  (polinomios de entrada com coeficientes de ate 3 digitos)");
    Console.WriteLine();
    Console.WriteLine("  grau   fator   passos   maior coeficiente (digitos)   com o conteudo tirado");

    foreach (var l in Medidas.Maximos())
        Console.WriteLine($"  {l.Grau,4} {l.GrauDoFator,7} {l.PassosIngenuo,8} {l.DigitosIngenuo,28} {l.DigitosPrimitivo,23}");

    Console.WriteLine();
}

static string Marca(bool valor) => valor ? "sim" : "NAO";
