import React, { useEffect, useState } from "react";
import './index.css'
import './Doador.css'




function Doador(){
    const [doador, setDoadores] = useState([]);

    useEffect(() => {
      fetch("http://localhost:3000/api/doadores") // backend rodando na porta 3000
        .then((res) => res.json())
        .then((data) => setDoadores(data))
        .catch((err) => console.error(err));
    }, []);



    return(
        <main>
            <h1 className="doador-titulo">Lista de Usuários:</h1>
            <section className="doador-lista">
                 <ul>
                    {doador.map((u,i) =>(
                        <li key={i}>
                            <div>Nome: {u.nome}</div> 
                            <span>Email: {u.email}</span>
                        </li>
                    )
                    )}
                </ul>  
            </section>
        </main>
    )
}

export default Doador;