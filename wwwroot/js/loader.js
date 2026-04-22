// Show loader when page starts loading
document.addEventListener("DOMContentLoaded", function () {
    const loader = document.getElementById("page-loader");

    // Wait until everything is loaded
    window.addEventListener("load", function () {
        setTimeout(() => {
            loader.classList.add("fade-out");

            setTimeout(() => {
                loader.style.display = "none";
            }, 500);
        }, 300); // small delay for smooth UX
    });
});
document.querySelectorAll("a").forEach(link => {
    link.addEventListener("click", function () {
        const loader = document.getElementById("page-loader");
        loader.style.display = "flex";
    });
});