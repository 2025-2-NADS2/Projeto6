import Redis from "ioredis";

let redis;

try {
    redis = new Redis({
        host: process.env.REDIS_HOST || "127.0.0.1",
        port: process.env.REDIS_PORT || 6379,
        retryStrategy: () => null // NÃO FICA TENTANDO INFINITO
    });

    redis.on("error", err => {
        console.log("⚠️ Redis offline. Continuando sem cache.");
    });
} catch (err) {
    console.log("⚠️ Redis não pôde iniciar.");
}

export default redis;
