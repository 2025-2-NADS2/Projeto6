using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace EntregaPOO
{
    internal class Atividades
    {
        int id;
        String descricao;
        DateTime data;

        public Atividades() { 
            this.id = 0;
            this.descricao = "Nova atividade";
            this.data = DateTime.Now;   
        }

        public int getId() { return this.id; }
        public String getDescricao() {  return this.descricao; }
        public DateTime getData() { return this.data;}

        public void setId(int id) { this.id = id;}
        public void setDescricao(String descricao) {  this.descricao = descricao;}
        public void setData(DateTime data) {  this.data = data;}
        
    }
}
