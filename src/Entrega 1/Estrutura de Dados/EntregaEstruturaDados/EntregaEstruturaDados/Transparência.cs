using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace EntregaEstruturaDados
{
    internal class Transparência
    {
        public String titulo {  get; set; }
        public int id {  get; set; }
        public string nome { get; set; } //Nome que salvarei no banco de dados, seguindo a regra de sem espaços e caracteres especiais

        public ArquivoPDF[] pdf { get; set; }
        public int quantArquivos {  get; set; } 

        public Transparência(String titulo, int id)
        {
            this.titulo = titulo;
            this.id = id;
            this.nome = "arquivo" + id; // variavel que ira otimizar a busca e captura do dado no banco de dados
            this.quantArquivos = 0;
            pdf = new ArquivoPDF[20];
        }

        public bool addPdf(int ID, String Nome)
        {
            ArquivoPDF npdf = new ArquivoPDF(ID, Nome);
            pdf[quantArquivos] = npdf;
            quantArquivos++;
            return true;
        }

        public void descrever()
        {
            Console.WriteLine("id: " + id + " | Titulo:" + titulo + " | nome formatado: " + nome);
            for (int i = 0; i < quantArquivos; i++)
            {
                Console.Write(pdf[i].id + "-");
                Console.WriteLine(pdf[i].nome);
                Console.WriteLine("¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨");
            }
        }

        public void bubbleSort()
        {
            int i, j;
            ArquivoPDF aux;
            int N = quantArquivos;

            for (i = N - 1; i > 0; i--)
            {
                for (j = 0; j < i; j++)
                {
                    if (pdf[j].Maior(pdf[j + 1]))
                    {
                        aux = pdf[j];
                        pdf[j] = pdf[j + 1];
                        pdf[j + 1] = aux;

                    }

                }

            }

        }

    }
}


    

