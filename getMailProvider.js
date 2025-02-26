function getMailProvider(email) {
    if (email.includes("@gmail.com")) return "https://mail.google.com";
    if (email.includes("@yahoo.com")) return "https://mail.yahoo.com";
    if (email.includes("@outlook.com") || email.includes("@hotmail.com")) return "https://outlook.live.com";
    return "https://www.google.com/search?q=ouvrir+email"; // Redirige vers Google si le fournisseur n'est pas détecté
}
