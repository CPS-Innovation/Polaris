interface Options {
    domain: string;
    userName: string;
    password: string;
    ntlmpacket: {
        target: Buffer;
        nonce: Buffer;
    };
}
declare class NTLMResponsePayload {
    data: Buffer;
    constructor(loginData: Options);
    toString(indent?: string): string;
    createResponse(challenge: Options): Buffer<ArrayBufferLike>;
    createClientNonce(): Buffer<ArrayBuffer>;
    ntlmv2Response(domain: string, user: string, password: string, serverNonce: Buffer, targetInfo: Buffer, clientNonce: Buffer, mytime: number): NonSharedBuffer;
    createTimestamp(time: number): Buffer<ArrayBuffer>;
    lmv2Response(domain: string, user: string, password: string, serverNonce: Buffer, clientNonce: Buffer): Buffer<ArrayBuffer>;
    ntv2Hash(domain: string, user: string, password: string): NonSharedBuffer;
    ntHash(text: string): Buffer<ArrayBuffer>;
    hmacMD5(data: Buffer, key: Buffer): NonSharedBuffer;
}
export default NTLMResponsePayload;
