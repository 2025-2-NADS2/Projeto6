import logoClarapqn from './assets/image/logo-clara-pqn.png'
import facebookIcon from './assets/image/facebook-icon.png'
import instagramIcon from './assets/image/instagram-icon.png'
import mapsimg from './assets/image/maps-img.png'

function Footer(){
    return(
    <footer>
        <img className='logo-footer' src={logoClarapqn} alt="Logo instituo alma" />
        <div>
            <p> Copyright &copy; Instituto Alma 2025. Todos os direitos reservados.</p>

            <div className='icon-footer'>
                <a href="https://maps.app.goo.gl/axMBEmGqhCJPXTvT9" target='_blank' rel='external'><img src={mapsimg} alt="Icone de redirecionamento ao endereço do instituo Alma"/></a>

                <a 
                href="https://www.facebook.com/almainstituto.oficial/" target='_blank' rel='external'>
                <img src={facebookIcon} alt="icone facebook" />
                </a>

                <a 
                href="https://www.instagram.com/almainstituto_oficial/?hl=en" target='_blank' rel='external'>
                <img src={instagramIcon} alt="icone instagram" />
                </a>
            </div>
            
        </div>
    </footer>
    )
}

export default Footer;