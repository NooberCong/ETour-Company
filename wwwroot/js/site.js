// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

function toggleNav(btn) {
    if (btn.classList.contains("is-active")) {
        document.getElementById("sideNav").style.width = "0";
        btn.classList.remove("is-active");
    } else {
        btn.classList.add("is-active");
        document.getElementById("sideNav").style.width = "calc(100vw / 3)";
    }
}

function onResizeRestoreSidebar() {
    if (document.documentElement.clientWidth >= 768 && document.getElementById("sideNav").style.width == "0px") {
        document.getElementById("sideNav").style.width = null;
    }
}

function closeSideNavOnClickOutside(e) {
    let btn = document.getElementById("burgerMenu");
    if (document.documentElement.clientWidth < 768 && btn.classList.contains("is-active")) {
        btn.click();
    }
}

function loopSound(fileName) {
    audio = new Audio(fileName);
    if (typeof audio.loop == 'boolean') {
        audio.loop = true;
    }
    else {
        audio.addEventListener('ended', function () {
            this.currentTime = 0;
            this.play();
        }, false);
    }
    document.body.addEventListener("mousemove", function () {
        if (audio.paused) {
            audio.play();
        }
    })
}

function createSpinnerFor(element) {
    let spinner = document.createElement("div");
    spinner.classList.add("spinner-grow", "text-center", "text-primary", ...element.classList);
    spinner.setAttribute("role", "status");
    let spinnerSize = Math.min(element.clientWidth, element.clientHeight);
    spinner.style.width = `${spinnerSize}px`;
    spinner.style.height = `${spinnerSize}px`;
    spinner.innerHTML = '<span class="sr-only">Loading...</span>'
    return spinner;
}

$(document).ready(function () {
    $(window).scroll(function () {
        if ($(this).scrollTop() > 50) {
            $('#back-to-top').fadeIn();
        } else {
            $('#back-to-top').fadeOut();
        }
    });
    // scroll body to 0px on click
    $('#back-to-top').click(function () {
        $('body,html').animate({
            scrollTop: 0
        }, 400);
        return false;
    });
});

function scheduleToast(toast) {
    let container = $('.toast-container')[0];
    container.appendChild(toast);
    $(toast).toast('show');
    setTimeout(() => {
        toast.remove();
    }, 2000);
}

function createToast(title, time, content, emoji = "✔", textClass = "success") {
    const toast = document.createElement("div");
    toast.classList.add("toast");
    toast.setAttribute("data-delay", "1500");
    toast.setAttribute("role", "alert");
    toast.setAttribute("aria-live", "assertive");
    toast.setAttribute("aria-atomic", "true");

    toast.innerHTML =
        `<div class="toast-header">
            <strong class="mr-auto">${title}</strong>
            <small>${time}</small>
            <button type="button" class="ml-2 mb-1 close" data-dismiss="toast" aria-label="Close">
                <span aria-hidden="true">&times;</span>
            </button>
        </div>
        <div class="toast-body text-${textClass}">
            ${content} ${emoji}
        </div>`;

    return toast;
}