import React, { useState } from "react";
import './Carousel.css'
//imagens
import carousel1 from './assets/image/carousel1.png'
import carousel2 from './assets/image/carousel2.png'
import carousel3 from './assets/image/carousel3.png'
import carousel4 from './assets/image/carousel4.png'
import carousel5 from './assets/image/carousel5.png'


const Carousel = () => {
  // Estado que guarda a imagem
  const [currentIndex, setCurrentIndex] = useState(0);

  // As imagens que vão aparecer no carrossel
  const images = [
    carousel1,
    carousel2,
    carousel3,
    carousel4,
    carousel5
  ];

  // Função para ir para a próxima imagem
  const nextSlide = () => {
    setCurrentIndex((prevIndex) =>
      prevIndex === images.length - 1 ? 0 : prevIndex + 1
    );
  };

  // Função para ir para a imagem anterior
  const prevSlide = () => {
    setCurrentIndex((prevIndex) =>
      prevIndex === 0 ? images.length - 1 : prevIndex - 1
    );
  };

  return (
    <div className="carousel">
      <div className="carousel-inner">
            <img
                src={images[currentIndex]}
                alt={`Execução das atividades ${currentIndex}`}
            />
            <button className="btn prev" onClick={prevSlide}>&lt;</button>
            <button className="btn next" onClick={nextSlide}>&gt;</button>

            <div className="dots">
                {images.map((_, index) => (
                <span key={index} className={`dot ${index === currentIndex ? "active" : ""}`}
                onClick={() => setCurrentIndex}>   
                </span>
             ))}
            </div>
        </div>

    </div>
  );
};

export default Carousel;
