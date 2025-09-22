using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntregaPOO
{
    internal class Users: Admin
    {
        float doacoes;
        bool voluntario;

        public float getDoacoes() {  return this.doacoes; }  
        public void setDoacoes(float doacoes) {  this.doacoes = doacoes; }
        public bool getVoluntario() { return this.voluntario; }
        public void setVoluntario(bool voluntario) {  this.voluntario = voluntario; }

        public void Doar()
        {
            Console.WriteLine("Digite o valor que deseja doar: ");
            float valor = float.Parse(Console.ReadLine());
            this.doacoes = valor;

        }
    }
}
