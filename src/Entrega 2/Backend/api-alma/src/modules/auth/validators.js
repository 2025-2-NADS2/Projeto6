// src/modules/auth/validators.js

// Funções utilitárias para validar email, senha forte e detectar e-mails descartáveis.

const disposableDomains = [
  // lista curta exemplo — você pode expandir ou carregar de arquivo/serviço
    'mailinator.com',
    'tempmail.com',
    '10minutemail.com',
    'guerrillamail.com'
]

// Verifica se string tem formato de e-mail válido (regex simples e eficiente)
export const isValidEmail = (email) => {
  // remove espaços e converte para minúsculas para validação
    const normalized = String(email).trim().toLowerCase()
  // regex que cobre a maioria dos e-mails válidos sem ser superrestritiva
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(normalized)
}

// Verifica se o domínio do email pertence à lista de e-mails descartáveis
export const isDisposableEmail = (email) => {
  // protege caso email seja undefined/null
    if (!email) return false
    const domain = String(email).trim().toLowerCase().split('@').pop()
    return disposableDomains.includes(domain)
}

// Verifica se a senha atende regras de segurança (mínimo 8, 1 maiúscula, 1 número, 1 símbolo)
export const isStrongPassword = (password) => {
    if (!password) return false
  // regex: pelo menos 8 chars, 1 uppercase, 1 digit, 1 special
    const strongPwd = /^(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).{8,}$/
    return strongPwd.test(String(password))
}
