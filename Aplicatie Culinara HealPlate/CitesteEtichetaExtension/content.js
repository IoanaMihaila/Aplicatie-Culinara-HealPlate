function extrageIngrediente() {
    const carduri = document.querySelectorAll(".ProductDefaultCard_name__A8sc4");
    const rezultate = [];

    for (let i = 0; i < Math.min(carduri.length, 5); i++) {
        const titlu = carduri[i].innerText.trim();
        if (titlu) {
            rezultate.push(titlu);
        }
    }

    return rezultate;
}

chrome.runtime.onMessage.addListener((request, sender, sendResponse) => {
    if (request.actiune === "extrageIngrediente") {
        const ingrediente = extrageIngrediente();
        sendResponse({ ingrediente });
    }
});
