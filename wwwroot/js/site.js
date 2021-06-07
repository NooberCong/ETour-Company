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