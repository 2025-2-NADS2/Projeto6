using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EntregaPOO
{
    internal class Admin
    {
        int id;
        String email;
        String senha;
        bool admin;

        public Admin(int id, String email, String senha) {
            this.id = id;
            this.email = email;
            this.senha = senha;
            this.admin = true;  
        }

        public Admin()
        {
            this.id=0;
            this.email = "teste@gmail.com";
            this.senha = "senha";
            this.admin = false;
        }

        public int getId() { return this.id;}
        public void setId(int id) { this.id = id;}
        public String getEmail() { return this.email;}
        public void setEmail(String email) {  this.email = email;}
        public String getSenha() {  return this.senha;}
        public void setSenha(String senha) {this.senha=senha;}
        public bool getAdmin() { return this.admin;}
        public void setAdmin(bool admin) { this.admin = admin;}

    }
}
