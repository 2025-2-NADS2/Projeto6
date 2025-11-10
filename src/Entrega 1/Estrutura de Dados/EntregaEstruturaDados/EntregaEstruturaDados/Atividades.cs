using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntregaEstruturaDados
{
    internal class Atividades
    {
        public int id {  get; set; }

        public String titulo { get; set; }
        public String descricao { get; set; }
        public String imagem { get; set; }

        public tipoAtv[] tAtv { get; set; }
        private int quantidadeAtv;

        public Atividades(int id, String titulo, String descricao, String imagem) {
        this.id = id;
        this.descricao = descricao; 
        this.imagem = imagem;
        this.titulo = titulo;
        tAtv = new tipoAtv[20];
        quantidadeAtv = 0;
        }

        public bool addTipoatv(int ID, String Nome)
        {
            tipoAtv atv = new tipoAtv(ID, Nome);
            tAtv[quantidadeAtv] = atv;
            quantidadeAtv++;
            return true;
        }


        public void descrever()
        {
            Console.WriteLine("id: " + id + "| Titulo:" + titulo + "| Descrição: " + descricao +"| imagem: " + imagem);
            for (int i = 0; i < quantidadeAtv; i++)
            {
                Console.Write(tAtv[i].id + "-");
                Console.WriteLine(tAtv[i].tipo);
                Console.WriteLine("¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨¨");
            }
        }

        public void bubbleSort()
        {
            int i, j;
            tipoAtv aux;
            int N = quantidadeAtv;

            for (i = N - 1; i > 0; i--)
            {
                for (j = 0; j < i; j++)
                {
                    if (tAtv[j].Maior(tAtv[j + 1]))
                    {
                        aux = tAtv[j];
                        tAtv[j] = tAtv[j + 1];
                        tAtv[j + 1] = aux;

                    }

                }

            }

        }

    }
}
