using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntregaEstruturaDados
{
    internal class tipoAtv
    {
        public int id { get; set; }
        public String tipo { get; set; }

        public tipoAtv(int id, string tipo)
        {
            this.id = id;
            this.tipo = tipo;
        }

        public bool Maior(tipoAtv atv)
        {
            return this.id > atv.id;

        }


    }
}
