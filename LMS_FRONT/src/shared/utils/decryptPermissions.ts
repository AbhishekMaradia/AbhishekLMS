/**
 * Decrypts AES-GCM encrypted permissions from the login response.
 * Returns a map of { moduleCode: permissionCode[] } for fast hasPermission() lookups.
 */
export async function decryptPermissions(encryptedBase64: string): Promise<Record<string, string[]>> {
    try {
        const keyBase64 = "LajL2QfHkbXKnypAR5PpXIqJpljJU12RYv4O0pFl4Hk=";
        const keyBuffer = Uint8Array.from(atob(keyBase64), c => c.charCodeAt(0));
        const combined = Uint8Array.from(atob(encryptedBase64), c => c.charCodeAt(0));

        const iv = combined.slice(0, 12);
        const tag = combined.slice(12, 28);
        const ciphertext = combined.slice(28);

        const dataToDecrypt = new Uint8Array(ciphertext.length + tag.length);
        dataToDecrypt.set(ciphertext);
        dataToDecrypt.set(tag, ciphertext.length);

        const cryptoKey = await window.crypto.subtle.importKey(
            "raw", keyBuffer, { name: "AES-GCM" }, false, ["decrypt"]
        );

        const decryptedBuffer = await window.crypto.subtle.decrypt(
            { name: "AES-GCM", iv, tagLength: 128 },
            cryptoKey,
            dataToDecrypt
        );

        const json = new TextDecoder().decode(decryptedBuffer);
        const raw = JSON.parse(json);

        if (Array.isArray(raw)) {
            const map: Record<string, string[]> = {};
            raw.forEach((p: any) => {
                const m = (p.moduleCode || p.ModuleCode || '').toUpperCase();
                const c = (p.permissionCode || p.PermissionCode || '').toUpperCase();
                if (m && c) {
                    if (!map[m]) map[m] = [];
                    if (!map[m].includes(c)) map[m].push(c);
                }
            });
            return map;
        }
        return raw;
    } catch (e) {
        console.error("Permission Decryption Error:", e);
        return {};
    }
}
