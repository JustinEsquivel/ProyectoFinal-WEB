// Mostrar / ocultar contraseña en los formularios de cuenta (adaptado de la plantilla Formly).
document.querySelectorAll("[data-toggle-password]").forEach(function (toggle) {
    toggle.addEventListener("click", function () {
        var input = document.querySelector(toggle.getAttribute("data-toggle-password"));
        if (!input) return;

        var show = input.type === "password";
        input.type = show ? "text" : "password";
        toggle.classList.toggle("eye", !show);
        toggle.classList.toggle("eye-closed", show);
        toggle.setAttribute("aria-label", show ? "Ocultar contraseña" : "Mostrar contraseña");
    });
});
