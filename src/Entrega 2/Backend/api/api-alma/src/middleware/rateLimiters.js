import rateLimit from "express-rate-limit";

// ✅ Função oficial segura para IPs IPv4/IPv6
const keyGenerator = rateLimit.ipKeyGenerator;

// ✅ Limite para login
export const loginLimiter = rateLimit({
    windowMs: 15 * 60 * 1000,
    max: 5,
    message: { message: "Muitas tentativas de login. Tente novamente mais tarde." },
    standardHeaders: true,
    legacyHeaders: false,
    keyGenerator, // ✅ usa função segura
});

// ✅ Limite para registro
export const registerLimiter = rateLimit({
    windowMs: 60 * 60 * 1000,
    max: 3,
    message: { message: "Muitas tentativas de registro a partir deste IP. Tente novamente mais tarde." },
    standardHeaders: true,
    legacyHeaders: false,
    keyGenerator, // ✅ idem
});
