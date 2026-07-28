// SkyOS — navigation, scroll reveal, reCAPTCHA
(function () {
  "use strict";

  var toggle = document.querySelector("[data-nav-toggle]");
  var nav = document.querySelector("[data-nav]");

  if (toggle && nav) {
    var setNavOpen = function (open) {
      nav.classList.toggle("open", open);
      toggle.setAttribute("aria-expanded", open ? "true" : "false");
      document.body.classList.toggle("nav-open", open);
    };

    toggle.addEventListener("click", function () {
      setNavOpen(!nav.classList.contains("open"));
    });

    nav.addEventListener("click", function (e) {
      if (!(e.target instanceof HTMLElement)) {
        return;
      }
      if (e.target.classList.contains("nav-link") || e.target.closest(".nav-menu-cta-btn")) {
        setNavOpen(false);
      }
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && nav.classList.contains("open")) {
        setNavOpen(false);
        toggle.focus();
      }
    });
  }

  // Language Dropdown Toggle
  var langWrap = document.querySelector("[data-lang-dropdown]");
  var langBtn = document.querySelector("[data-lang-btn]");
  if (langWrap && langBtn) {
    langBtn.addEventListener("click", function (e) {
      e.stopPropagation();
      var open = langWrap.classList.toggle("is-open");
      langBtn.setAttribute("aria-expanded", open ? "true" : "false");
    });

    document.addEventListener("click", function (e) {
      if (!langWrap.contains(e.target)) {
        langWrap.classList.remove("is-open");
        langBtn.setAttribute("aria-expanded", "false");
      }
    });

    document.addEventListener("keydown", function (e) {
      if (e.key === "Escape" && langWrap.classList.contains("is-open")) {
        langWrap.classList.remove("is-open");
        langBtn.setAttribute("aria-expanded", "false");
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

  // Full-screen hero slider — fixed 5s interval (no hover pause; it made timing inconsistent)
  var slider = document.querySelector("[data-hero-slider]");
  if (slider && slider.getAttribute("data-slider-ready") !== "1") {
    slider.setAttribute("data-slider-ready", "1");
    var slides = Array.prototype.slice.call(slider.querySelectorAll("[data-slide]"));
    var dotsWrap = slider.querySelector("[data-slider-dots]");
    var progress = slider.querySelector("[data-slider-progress]");
    var prevBtn = slider.querySelector("[data-slider-prev]");
    var nextBtn = slider.querySelector("[data-slider-next]");
    var index = Math.max(0, slides.findIndex(function (s) { return s.classList.contains("is-active"); }));
    var autoTimer = null;
    var progressRaf = null;
    var deadline = 0;
    var duration = 5000;
    var reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;

    if (index < 0) { index = 0; }

    if (dotsWrap) {
      var slideLabel = slider.getAttribute("data-slide-label") || "Slayt";
      slides.forEach(function (_, i) {
        var dot = document.createElement("button");
        dot.type = "button";
        dot.className = "hero-dot" + (i === index ? " is-active" : "");
        dot.setAttribute("aria-label", slideLabel + " " + (i + 1));
        dot.addEventListener("click", function () { goTo(i); });
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

    function clearTimers() {
      if (autoTimer) {
        window.clearTimeout(autoTimer);
        autoTimer = null;
      }
      if (progressRaf) {
        window.cancelAnimationFrame(progressRaf);
        progressRaf = null;
      }
    }

    function tickProgress() {
      if (!progress || reduced || !deadline) { return; }
      var remaining = Math.max(0, deadline - Date.now());
      var pct = Math.min(100, ((duration - remaining) / duration) * 100);
      var stepped = Math.round(pct / 10) * 10;
      progress.setAttribute("data-pct", String(stepped));
      if (remaining > 0) {
        progressRaf = window.requestAnimationFrame(tickProgress);
      } else {
        progress.setAttribute("data-pct", "100");
      }
    }

    function goTo(i) {
      if (!slides.length) { return; }
      setActive((i + slides.length) % slides.length);
      armTimer();
    }

    function armTimer() {
      clearTimers();
      if (progress) { progress.setAttribute("data-pct", "0"); }
      if (slides.length < 2 || document.hidden) {
        deadline = 0;
        return;
      }

      deadline = Date.now() + duration;
      autoTimer = window.setTimeout(function () {
        autoTimer = null;
        goTo(index + 1);
      }, duration);

      if (!reduced && progress) {
        progressRaf = window.requestAnimationFrame(tickProgress);
      }
    }

    if (prevBtn) { prevBtn.addEventListener("click", function () { goTo(index - 1); }); }
    if (nextBtn) { nextBtn.addEventListener("click", function () { goTo(index + 1); }); }

    slider.addEventListener("keydown", function (e) {
      if (e.key === "ArrowLeft") { goTo(index - 1); }
      if (e.key === "ArrowRight") { goTo(index + 1); }
    });

    document.addEventListener("visibilitychange", function () {
      if (document.hidden) {
        clearTimers();
        deadline = 0;
      } else {
        armTimer();
      }
    });

    setActive(index);
    armTimer();
  }

  // reCAPTCHA v3 (contact + feedback forms)
  function loadRecaptcha(siteKey) {
    return new Promise(function (resolve, reject) {
      if (!siteKey) {
        resolve();
        return;
      }

      if (window.grecaptcha && window.grecaptcha.execute) {
        window.grecaptcha.ready(resolve);
        return;
      }

      var existing = document.querySelector('script[data-recaptcha-loader="1"]');
      if (existing) {
        existing.addEventListener("load", function () { window.grecaptcha.ready(resolve); });
        existing.addEventListener("error", reject);
        return;
      }

      var script = document.createElement("script");
      script.src = "https://www.google.com/recaptcha/api.js?render=" + encodeURIComponent(siteKey);
      script.async = true;
      script.defer = true;
      script.setAttribute("data-recaptcha-loader", "1");
      script.onload = function () { window.grecaptcha.ready(resolve); };
      script.onerror = reject;
      document.head.appendChild(script);
    });
  }

  function refreshRecaptchaToken(form, siteKey, action, tokenField) {
    return window.grecaptcha.execute(siteKey, { action: action }).then(function (token) {
      tokenField.value = token;
      form.dataset.captchaIssuedAt = String(Date.now());
      return token;
    });
  }

  document.querySelectorAll("[data-recaptcha-form]").forEach(function (form) {
    var siteKey = form.getAttribute("data-recaptcha-key");
    var recaptchaAction = form.getAttribute("data-recaptcha-action") || "contact";
    var tokenField = form.querySelector("[data-recaptcha-token]");
    var errorBox = form.querySelector("[data-recaptcha-error]");
    var submitButton = form.querySelector('[type="submit"]');

    if (!siteKey || !tokenField) {
      return;
    }

    var showError = function (message) {
      if (!errorBox) {
        return;
      }
      errorBox.textContent = message;
      errorBox.hidden = false;
    };

    var clearError = function () {
      if (!errorBox) {
        return;
      }
      errorBox.textContent = "";
      errorBox.hidden = true;
    };

    var setSubmitting = function (isSubmitting) {
      if (!submitButton) {
        return;
      }
      submitButton.disabled = isSubmitting;
      submitButton.setAttribute("aria-busy", isSubmitting ? "true" : "false");
    };

    loadRecaptcha(siteKey).then(function () {
      return refreshRecaptchaToken(form, siteKey, recaptchaAction, tokenField);
    }).catch(function () {
      showError(form.getAttribute("data-recaptcha-load-error") || "Security verification could not be loaded.");
    });

    window.setInterval(function () {
      if (!window.grecaptcha) {
        return;
      }
      refreshRecaptchaToken(form, siteKey, recaptchaAction, tokenField).catch(function () { /* ignore background refresh errors */ });
    }, 90000);

    form.addEventListener("submit", function (e) {
      if (form.dataset.captchaReady === "1") {
        form.dataset.captchaReady = "0";
        return;
      }

      e.preventDefault();
      clearError();
      setSubmitting(true);

      var issuedAt = Number(form.dataset.captchaIssuedAt || "0");
      var tokenIsFresh = tokenField.value && (Date.now() - issuedAt) < 110000;

      var submitWithToken = function () {
        form.dataset.captchaReady = "1";
        if (form.requestSubmit) {
          form.requestSubmit();
        } else {
          form.submit();
        }
      };

      var fail = function () {
        setSubmitting(false);
        showError(form.getAttribute("data-recaptcha-submit-error") || "Security verification failed. Please try again.");
      };

      loadRecaptcha(siteKey).then(function () {
        if (tokenIsFresh) {
          submitWithToken();
          return;
        }
        return refreshRecaptchaToken(form, siteKey, recaptchaAction, tokenField).then(submitWithToken);
      }).catch(fail);
    });
  });
  // Back to top — show after ~400px scroll
  var backToTop = document.querySelector("[data-back-to-top]");
  if (backToTop) {
    var backToTopThreshold = 400;
    var backToTopVisible = false;
    var setBackToTop = function () {
      var show = window.scrollY > backToTopThreshold;
      if (show === backToTopVisible) {
        return;
      }
      backToTopVisible = show;
      backToTop.classList.toggle("is-visible", show);
      backToTop.setAttribute("aria-hidden", show ? "false" : "true");
      backToTop.tabIndex = show ? 0 : -1;
    };
    setBackToTop();
    window.addEventListener("scroll", setBackToTop, { passive: true });
    backToTop.addEventListener("click", function () {
      var reduced = window.matchMedia("(prefers-reduced-motion: reduce)").matches;
      window.scrollTo({ top: 0, behavior: reduced ? "auto" : "smooth" });
    });
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

  // Footer accordion (mobile compact sections)
  document.querySelectorAll("[data-footer-accordion]").forEach(function (section) {
    var trigger = section.querySelector("[data-footer-trigger]");
    var panel = section.querySelector("[data-footer-panel]");
    var icon = section.querySelector(".footer-accordion-icon");
    if (!trigger || !panel) {
      return;
    }

    trigger.addEventListener("click", function () {
      if (window.matchMedia("(min-width: 992px)").matches) {
        return;
      }

      var open = !section.classList.contains("is-open");
      section.classList.toggle("is-open", open);
      trigger.setAttribute("aria-expanded", open ? "true" : "false");
      if (open) {
        panel.removeAttribute("hidden");
      } else {
        panel.setAttribute("hidden", "");
      }
      if (icon) {
        icon.textContent = open ? "−" : "+";
      }
    });
  });

  // Team member profile dialogs
  (function initMemberDialogs() {
    var openers = document.querySelectorAll("[data-member-open]");
    if (!openers.length || typeof HTMLDialogElement === "undefined") {
      return;
    }

    var lastOpener = null;

    var closeDialog = function (dialog) {
      if (!dialog || !dialog.open) {
        return;
      }
      dialog.close();
      document.body.classList.remove("member-dialog-open");
      if (lastOpener && typeof lastOpener.focus === "function") {
        lastOpener.focus();
      }
    };

    var openByTarget = function (id, opener) {
      var dialog = id ? document.getElementById(id) : null;
      if (!(dialog instanceof HTMLDialogElement)) {
        return;
      }
      lastOpener = opener || null;
      document.body.classList.add("member-dialog-open");
      dialog.showModal();
      var closeBtn = dialog.querySelector("[data-member-close]");
      if (closeBtn) {
        closeBtn.focus();
      }
    };

    openers.forEach(function (btn) {
      btn.addEventListener("click", function (e) {
        e.stopPropagation();
        openByTarget(btn.getAttribute("data-member-target"), btn);
      });
    });

    document.querySelectorAll(".member-card--interactive").forEach(function (card) {
      card.addEventListener("click", function () {
        var btn = card.querySelector("[data-member-open]");
        if (btn) {
          openByTarget(btn.getAttribute("data-member-target"), btn);
        }
      });
    });

    document.querySelectorAll(".member-dialog").forEach(function (dialog) {
      dialog.addEventListener("click", function (e) {
        if (e.target === dialog) {
          closeDialog(dialog);
        }
      });

      dialog.querySelectorAll("[data-member-close]").forEach(function (btn) {
        btn.addEventListener("click", function () {
          closeDialog(dialog);
        });
      });

      dialog.addEventListener("close", function () {
        document.body.classList.remove("member-dialog-open");
      });
    });
  })();
})();
