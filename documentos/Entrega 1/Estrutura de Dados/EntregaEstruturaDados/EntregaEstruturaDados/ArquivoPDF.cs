using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntregaEstruturaDados
{
    internal class ArquivoPDF
    {
        public int id { get; set; }
        public String nome { get; set; }

        public ArquivoPDF(int id, string nome)
        {
            this.id = id;
            this.nome = nome;
        }

        public bool Maior(ArquivoPDF pdf)
        {
            return this.id > pdf.id;

        }
    }
}
