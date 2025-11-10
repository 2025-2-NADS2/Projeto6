// App.jsx
import './index.css'
import Header from './Header'
import Footer from './footer'
import Carousel from './Carousel'
import Doador from './Doador'
import { Routes, Route } from "react-router-dom";

//import de imagens
import imagemPrincipal from './assets/image/imagem-principal.JPG'
import imgCrescer from './assets/image/crescer-img.png'
import imgAlimentar from './assets/image/alimentar-img.png'
import imgNatalAmor from './assets/image/natalAmor-img.png'
import apo1 from './assets/image/apoiador-1.png'
import apo2 from './assets/image/apoiador-2.png'
import apo3 from './assets/image/apoiador-3.png'
import apo4 from './assets/image/apoiador-4.png'
import apo5 from './assets/image/apoiador-5.png'

// Home separado
function Home() {
  return (
    <main>
      <section className='home-section1'>
        <div className="home-primeira">
          <div className="home-primeira-texto">
            <h1>Instituto Alma</h1>
            <p>
              No Instituto Alma, acreditamos que a verdadeira transformação social acontece quando levamos esperança, dignidade e carinho a quem mais precisa. Nossos projetos são criados para ir além, oferecendo experiências únicas que tocam o coração e mudam histórias. Junte-se a nós nessa missão de construir um futuro mais justo e humano para todos.
            </p>
            <div>
              <button className='btn-padrao'>saiba mais</button>
              <button className="btn-destaque">Doe agora</button>
            </div>
          </div>
          <img className="image-home" src={imagemPrincipal} alt="Sopa comunitaria"/>
        </div>
      </section>

      <section className='home-section2'>
        <h1>ATIVIDADES</h1>
        <div>
          <article>
            <img className='imgAlimentar' src={imgAlimentar} alt="equipe de cozinha" />
            <h2>Projeto Alimentar</h2>
            <p>
              Toda semana, levamos mais que refeições: entregamos carinho e dignidade. Com ingredientes de qualidade e muito cuidado, preparamos 2 mil pratos de sopa e 200 cestas básicas, nutrindo o corpo e a alma.
            </p>
            <button className="btn-destaque">Saiba mais</button>
          </article>
          <article>
            <img className='imgCreser' src={imgCrescer} alt="fundador do alma segurando caixas de jogos" />
            <h2>Projeto Crê.Ser</h2>
            <p>
              Apoiamos gestantes em vulnerabilidade do início da gravidez até o sexto mês do bebê, oferecendo afeto, informações e o suporte necessário para uma gestação saudável e um futuro com mais esperança.
            </p>
            <button className="btn-destaque">Saiba mais</button>
          </article>
          <article>
            <img className='natalAmor' src={imgNatalAmor} alt="mãe e filho se abraçando" />
            <h2>Projeto Natal do Amor</h2>
            <p>
              Transformamos o Natal de 2.500 crianças. Em uma loja de brinquedos especial, elas têm a liberdade de escolher o próprio presente, devolvendo a magia da data e a alegria de ter um sonho realizado.
            </p>
            <button className="btn-destaque">Saiba mais</button>
          </article>
        </div>
      </section>

      <section className='home-section3'>
        <article>
          <h1>Nossos Eventos</h1>
          <p>
          Ao longo do ano, realizamos eventos especiais que unem solidariedade, alegria e transformação. Cada ação é pensada para aproximar a comunidade, fortalecer vínculos e levar esperança a quem mais precisa.
          Promovemos encontros que celebram a vida, distribuem sorrisos e criam memórias inesquecíveis, sempre com o objetivo de impactar positivamente a sociedade.
          Com a participação de voluntários e parceiros, nossos eventos se tornam oportunidades únicas de partilha, cuidado e amor ao próximo.
          </p>
        </article>
        <Carousel/>
      </section>

      <section className='home-section4'>
        <h1>APOIADORES</h1>
        <div>
          <img src={apo1} alt="Mocoto bar e restaurante" />
          <img src={apo2} alt="Azon acessoria esportiva" />
          <img src={apo3} alt="Cacau Show" />
          <img src={apo4} alt="Apoio estrategico" />
          <img src={apo5} alt="Big Pão express" />
        </div>
      </section>
    </main>
  )
}

// App.jsx principal
function App() {
  return (
    <>
      <Header/>

      {/* Aqui o React Router controla qual conteúdo mostrar */}
      <Routes>
        <Route path="/" element={<Home />} />
        <Route path="/doador" element={<Doador />} />
      </Routes>

      <Footer/>
    </>
  )
}

export default App

