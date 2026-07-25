// SkyOS — navigation, scroll reveal, reCAPTCHA
(function () {
  "use strict";

  var toggle = document.querySelector("[data-nav-toggle]");
  var nav = document.querySelector("[data-nav]");

  if (toggle && nav) {
    toggle.addEventListener("click", function () {
      var open = nav.classList.toggle("open");
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
    });

    nav.addEventListener("click", function (e) {
      if (e.target instanceof HTMLElement && e.target.classList.contains("nav-link")) {
        nav.classList.remove("open");
        toggle.setAttribute("aria-expanded", "false");
      }
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && nav.classList.contains("open")) {
        nav.classList.remove("open");
        toggle.setAttribute("aria-expanded", "false");
        toggle.focus();
      }
    });
  }

  // Scroll-triggered reveals (IntersectionObserver)
  var nodes = document.querySelectorAll("[data-reveal]");
  if (nodes.length && "IntersectionObserver" in window) {
    var io = new IntersectionObserver(function (entries) {
      entries.forEach(function (entry) {
        if (entry.isIntersecting) {
          entry.target.classList.add("is-in");
          io.unobserve(entry.target);
        }
      });
    }, { rootMargin: "0px 0px -8% 0px", threshold: 0.12 });

    nodes.forEach(function (el, i) {
      el.setAttribute("data-reveal-delay", Math.min(i % 4, 3));
      io.observe(el);
    });
  } else {
    nodes.forEach(function (el) { el.classList.add("is-in"); });
  }

  // Header elevation on scroll
  var header = document.querySelector("[data-header]");
  if (header) {
    var onScroll = function () {
      header.classList.toggle("is-scrolled", window.scrollY > 24);
    };
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
  }

  // Full-screen hero slider
  var slider = document.querySelector("[data-hero-slider]");
  if (slider) {
    var slides = Array.prototype.slice.call(slider.querySelectorAll("[data-slide]"));
    var dotsWrap = slider.querySelector("[data-slider-dots]");
    var progress = slider.querySelector("[data-slider-progress]");
    var prevBtn = slider.querySelector("[data-slider-prev]");
    var nextBtn = slider.querySelector("[data-slider-next]");
    var index = Math.max(0, slides.findIndex(function (s) { return s.classList.contains("is-active"); }));
    var timer = null;
    var startedAt = 0;
    var duration = 5000;
    var reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (dotsWrap) {
      var slideLabel = slider.getAttribute("data-slide-label") || "Slayt";
      slides.forEach(function (_, i) {
        var dot = document.createElement("button");
        dot.type = "button";
        dot.className = "hero-dot" + (i === index ? " is-active" : "");
        dot.setAttribute("aria-label", slideLabel + " " + (i + 1));
        dot.addEventListener("click", function () { goTo(i, true); });
        dotsWrap.appendChild(dot);
      });
    }

    function setActive(i) {
      slides.forEach(function (slide, n) {
        var on = n === i;
        slide.classList.toggle("is-active", on);
        slide.setAttribute("aria-hidden", on ? "false" : "true");
      });
      if (dotsWrap) {
        Array.prototype.forEach.call(dotsWrap.children, function (dot, n) {
          dot.classList.toggle("is-active", n === i);
        });
      }
      index = i;
    }

    function goTo(i, user) {
      if (!slides.length) { return; }
      setActive((i + slides.length) % slides.length);
      restart(user);
    }

    function tickProgress() {
      if (!progress || reduced) { return; }
      var elapsed = Date.now() - startedAt;
      var pct = Math.min(100, (elapsed / duration) * 100);
      // Use data attribute instead of inline style to avoid CSP inline style violation
      progress.setAttribute("data-pct", Math.round(pct));
      if (pct < 100) {
        timer = window.requestAnimationFrame(tickProgress);
      } else {
        goTo(index + 1, false);
      }
    }

    function restart() {
      if (timer) {
        window.cancelAnimationFrame(timer);
        timer = null;
      }
      if (progress) { progress.setAttribute("data-pct", "0"); }
      if (reduced || slides.length < 2) { return; }
      startedAt = Date.now();
      timer = window.requestAnimationFrame(tickProgress);
    }

    if (prevBtn) { prevBtn.addEventListener("click", function () { goTo(index - 1, true); }); }
    if (nextBtn) { nextBtn.addEventListener("click", function () { goTo(index + 1, true); }); }

    slider.addEventListener("keydown", function (e) {
      if (e.key === "ArrowLeft") { goTo(index - 1, true); }
      if (e.key === "ArrowRight") { goTo(index + 1, true); }
    });

    slider.addEventListener("mouseenter", function () {
      if (timer) { window.cancelAnimationFrame(timer); timer = null; }
    });
    slider.addEventListener("mouseleave", function () { restart(); });

    document.addEventListener("visibilitychange", function () {
      if (document.hidden) {
        if (timer) { window.cancelAnimationFrame(timer); timer = null; }
      } else {
        restart();
      }
    });

    setActive(index);
    restart();
  }

  // reCAPTCHA v3
  var form = document.querySelector("[data-contact-form]");
  if (form) {
    var siteKey = form.getAttribute("data-recaptcha-key");
    var tokenField = form.querySelector("[data-recaptcha-token]");

    // siteKey yoksa reCAPTCHA devre dışı — formu direkt gönder
    if (siteKey && tokenField) {
      form.addEventListener("submit", function (e) {
        if (form.dataset.captchaReady === "1") {
          return; // token zaten hazır, ikinci submit — engelleme
        }
        e.preventDefault();

        // grecaptcha henüz yüklenmemiş olabilir (async defer), ready() ile bekle
        var execute = function () {
          window.grecaptcha.execute(siteKey, { action: "contact" }).then(function (token) {
            tokenField.value = token;
            form.dataset.captchaReady = "1";
            if (form.requestSubmit) {
              form.requestSubmit();
            } else {
              form.submit();
            }
          }).catch(function () {
            // Token alınamazsa formu bloke etme, doğrudan gönder
            form.dataset.captchaReady = "1";
            form.submit();
          });
        };

        if (window.grecaptcha) {
          window.grecaptcha.ready(execute);
        } else {
          // grecaptcha script daha yüklenmedi — yüklenince çalıştır
          var interval = setInterval(function () {
            if (window.grecaptcha) {
              clearInterval(interval);
              window.grecaptcha.ready(execute);
            }
          }, 100);
          // 5 saniye sonra hala yüklenmediyse formu direkt gönder
          setTimeout(function () {
            clearInterval(interval);
            if (!window.grecaptcha) {
              form.dataset.captchaReady = "1";
              form.submit();
            }
          }, 5000);
        }
      });
    }
  }
  // Infinite marquee — clones content so track is always wider than viewport
  document.querySelectorAll("[data-marquee]").forEach(function (strip) {
    var track = strip.querySelector(".marquee-track");
    if (!track || track.dataset.ready === "1") {
      return;
    }
    var html = track.innerHTML;
    // Ensure enough width for seamless -50% loop on ultrawide screens
    while (track.scrollWidth < window.innerWidth * 2.2) {
      track.insertAdjacentHTML("beforeend", html);
      if (track.children.length > 64) {
        break;
      }
    }
    track.dataset.ready = "1";
  });
})();
