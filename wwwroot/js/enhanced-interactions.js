/* ==========================================
   تحسينات التفاعل والحركات
   Enhanced Interactions & Animations
   Version: 1.0
   ========================================== */

document.addEventListener('DOMContentLoaded', function () {

    // ==========================================
    // 1. تحسينات الأزرار والتفاعل
    // ==========================================

    // إضافة تأثيرات الضغط للأزرار
    const buttons = document.querySelectorAll('.btn');
    buttons.forEach(button => {
        button.addEventListener('click', function (e) {
            // إنشاء تأثير الموجة
            const ripple = document.createElement('span');
            const rect = this.getBoundingClientRect();
            const size = Math.max(rect.width, rect.height);
            const x = e.clientX - rect.left - size / 2;
            const y = e.clientY - rect.top - size / 2;

            ripple.style.width = ripple.style.height = size + 'px';
            ripple.style.left = x + 'px';
            ripple.style.top = y + 'px';
            ripple.classList.add('ripple');

            this.appendChild(ripple);

            setTimeout(() => {
                ripple.remove();
            }, 600);
        });
    });

    // ==========================================
    // 2. تحسينات النماذج
    // ==========================================

    // تحسين تفاعل حقول الإدخال
    const formControls = document.querySelectorAll('.form-control, .form-select');
    formControls.forEach(control => {
        // إضافة تأثير التركيز
        control.addEventListener('focus', function () {
            this.parentElement.classList.add('focused');
        });

        control.addEventListener('blur', function () {
            this.parentElement.classList.remove('focused');
            if (this.value.trim() !== '') {
                this.parentElement.classList.add('has-value');
            } else {
                this.parentElement.classList.remove('has-value');
            }
        });

        // فحص القيمة الأولية
        if (control.value.trim() !== '') {
            control.parentElement.classList.add('has-value');
        }
    });

    // تحسين رسائل التحقق
    const invalidInputs = document.querySelectorAll('.is-invalid');
    invalidInputs.forEach(input => {
        input.addEventListener('input', function () {
            if (this.value.trim() !== '') {
                this.classList.remove('is-invalid');
                const feedback = this.parentElement.querySelector('.invalid-feedback');
                if (feedback) {
                    feedback.style.display = 'none';
                }
            }
        });
    });

    // ==========================================
    // 3. تحسينات البطاقات والكروت
    // ==========================================

    // إضافة تأثيرات الحركة للبطاقات
    const cards = document.querySelectorAll('.card, .request-card, .store-card');
    cards.forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-4px)';
            this.style.boxShadow = '0 8px 24px rgba(0, 0, 0, 0.15)';
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = 'translateY(0)';
            this.style.boxShadow = '0 4px 12px rgba(0, 0, 0, 0.08)';
        });
    });

    // ==========================================
    // 4. تحسينات التنقل والقوائم
    // ==========================================

    // تحسين القوائم المنسدلة
    const dropdowns = document.querySelectorAll('.dropdown-toggle');
    dropdowns.forEach(dropdown => {
        dropdown.addEventListener('click', function (e) {
            e.preventDefault();
            const menu = this.nextElementSibling;
            if (menu && menu.classList.contains('dropdown-menu')) {
                menu.classList.toggle('show');

                // إغلاق القوائم الأخرى
                document.querySelectorAll('.dropdown-menu.show').forEach(otherMenu => {
                    if (otherMenu !== menu) {
                        otherMenu.classList.remove('show');
                    }
                });
            }
        });
    });

    // إغلاق القوائم عند النقر خارجها
    document.addEventListener('click', function (e) {
        if (!e.target.closest('.dropdown')) {
            document.querySelectorAll('.dropdown-menu.show').forEach(menu => {
                menu.classList.remove('show');
            });
        }
    });

    // ==========================================
    // 5. تحسينات التمرير والتنقل السلس
    // ==========================================

    // تمرير سلس للروابط الداخلية
    const smoothScrollLinks = document.querySelectorAll('a[href^="#"]');
    smoothScrollLinks.forEach(link => {
        link.addEventListener('click', function (e) {
            const targetId = this.getAttribute('href');
            if (targetId === '#') return;

            const targetElement = document.querySelector(targetId);
            if (targetElement) {
                e.preventDefault();
                targetElement.scrollIntoView({
                    behavior: 'smooth',
                    block: 'start'
                });
            }
        });
    });

    // إظهار زر العودة للأعلى
    const backToTopBtn = createBackToTopButton();

    window.addEventListener('scroll', function () {
        if (window.pageYOffset > 300) {
            backToTopBtn.classList.add('show');
        } else {
            backToTopBtn.classList.remove('show');
        }
    });

    // ==========================================
    // 6. تحسينات الصور والوسائط
    // ==========================================

    // تحميل الصور بشكل تدريجي (Lazy Loading)
    const images = document.querySelectorAll('img[data-src]');
    const imageObserver = new IntersectionObserver((entries, observer) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                const img = entry.target;
                img.src = img.dataset.src;
                img.classList.remove('lazy');
                imageObserver.unobserve(img);
            }
        });
    });

    images.forEach(img => imageObserver.observe(img));

    // تحسين عرض الصور في modal
    const imageLinks = document.querySelectorAll('a[data-bs-toggle="modal"][data-bs-target="#imageModal"]');
    imageLinks.forEach(link => {
        link.addEventListener('click', function () {
            const imageSrc = this.getAttribute('href') || this.querySelector('img')?.src;
            const modal = document.querySelector('#imageModal');
            if (modal && imageSrc) {
                const modalImage = modal.querySelector('.modal-body img');
                if (modalImage) {
                    modalImage.src = imageSrc;
                }
            }
        });
    });

    // ==========================================
    // 7. تحسينات الإشعارات والتنبيهات
    // ==========================================

    // إخفاء التنبيهات تلقائياً
    const alerts = document.querySelectorAll('.alert:not(.alert-permanent)');
    alerts.forEach(alert => {
        setTimeout(() => {
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                alert.remove();
            }, 300);
        }, 5000);
    });

    // تحسين إغلاق التنبيهات
    const alertCloseButtons = document.querySelectorAll('.alert .btn-close');
    alertCloseButtons.forEach(button => {
        button.addEventListener('click', function () {
            const alert = this.closest('.alert');
            alert.style.opacity = '0';
            alert.style.transform = 'translateY(-20px)';
            setTimeout(() => {
                alert.remove();
            }, 300);
        });
    });

    // ==========================================
    // 8. تحسينات الجداول
    // ==========================================

    // تحسين الجداول المتجاوبة
    const tables = document.querySelectorAll('.table-responsive table');
    tables.forEach(table => {
        // إضافة خصائص للجداول المكدسة على الموبايل
        if (window.innerWidth <= 576) {
            table.classList.add('table-stack');
            const headers = table.querySelectorAll('thead th');
            const cells = table.querySelectorAll('tbody td');

            cells.forEach((cell, index) => {
                const headerIndex = index % headers.length;
                if (headers[headerIndex]) {
                    cell.setAttribute('data-label', headers[headerIndex].textContent);
                }
            });
        }
    });

    // ==========================================
    // 9. تحسينات الأداء والتحميل
    // ==========================================

    // إظهار مؤشر التحميل للنماذج
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        form.addEventListener('submit', function () {
            const submitBtn = this.querySelector('button[type="submit"], input[type="submit"]');
            if (submitBtn) {
                submitBtn.disabled = true;
                submitBtn.innerHTML = '<i class="fas fa-spinner fa-spin me-2"></i>جاري التحميل...';
            }
        });
    });

    // تحسين تحميل المحتوى بشكل تدريجي
    const lazyElements = document.querySelectorAll('.lazy-load');
    const elementObserver = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('loaded');
                elementObserver.unobserve(entry.target);
            }
        });
    });

    lazyElements.forEach(element => elementObserver.observe(element));

    // ==========================================
    // 10. وظائف مساعدة
    // ==========================================

    // إنشاء زر العودة للأعلى
    function createBackToTopButton() {
        const button = document.createElement('button');
        button.innerHTML = '<i class="fas fa-arrow-up"></i>';
        button.className = 'back-to-top';
        button.setAttribute('aria-label', 'العودة للأعلى');

        button.addEventListener('click', function () {
            window.scrollTo({
                top: 0,
                behavior: 'smooth'
            });
        });

        document.body.appendChild(button);
        return button;
    }

    // تحسين الأداء للأحداث
    function throttle(func, wait) {
        let timeout;
        return function executedFunction(...args) {
            const later = () => {
                clearTimeout(timeout);
                func(...args);
            };
            clearTimeout(timeout);
            timeout = setTimeout(later, wait);
        };
    }

    // تحسين أحداث التمرير
    const optimizedScrollHandler = throttle(function () {
        // معالجة أحداث التمرير المحسنة
        const scrollTop = window.pageYOffset;

        // إخفاء/إظهار شريط التنقل
        const navbar = document.querySelector('.navbar');
        if (navbar) {
            if (scrollTop > 100) {
                navbar.classList.add('navbar-scrolled');
            } else {
                navbar.classList.remove('navbar-scrolled');
            }
        }

        // تحديث مؤشر التقدم
        const progressBar = document.querySelector('.reading-progress');
        if (progressBar) {
            const windowHeight = document.documentElement.scrollHeight - window.innerHeight;
            const progress = (scrollTop / windowHeight) * 100;
            progressBar.style.width = progress + '%';
        }
    }, 100);

    window.addEventListener('scroll', optimizedScrollHandler);

    // ==========================================
    // 11. تحسينات إضافية للموبايل
    // ==========================================

    // تحسين اللمس للأجهزة المحمولة
    if ('ontouchstart' in window) {
        document.body.classList.add('touch-device');

        // تحسين النقر للأزرار
        const touchButtons = document.querySelectorAll('.btn, .card, .nav-link');
        touchButtons.forEach(button => {
            button.addEventListener('touchstart', function () {
                this.classList.add('touch-active');
            });

            button.addEventListener('touchend', function () {
                setTimeout(() => {
                    this.classList.remove('touch-active');
                }, 150);
            });
        });
    }

    // تحسين التوجه للشاشة
    window.addEventListener('orientationchange', function () {
        setTimeout(() => {
            // إعادة حساب الأبعاد بعد تغيير التوجه
            window.dispatchEvent(new Event('resize'));
        }, 100);
    });

    // تحسين حجم الشاشة
    window.addEventListener('resize', throttle(function () {
        // تحديث التخطيط حسب حجم الشاشة
        const isMobile = window.innerWidth <= 768;
        document.body.classList.toggle('mobile-view', isMobile);
        document.body.classList.toggle('desktop-view', !isMobile);
    }, 250));

    // تطبيق التصنيف الأولي
    const isMobile = window.innerWidth <= 768;
    document.body.classList.toggle('mobile-view', isMobile);
    document.body.classList.toggle('desktop-view', !isMobile);
});

// ==========================================
// CSS للتأثيرات الإضافية
// ==========================================

// إضافة الأنماط المطلوبة للتأثيرات
const style = document.createElement('style');
style.textContent = `
    /* تأثير الموجة للأزرار */
    .btn {
        position: relative;
        overflow: hidden;
    }
    
    .ripple {
        position: absolute;
        border-radius: 50%;
        background: rgba(255, 255, 255, 0.6);
        transform: scale(0);
        animation: ripple-animation 0.6s linear;
        pointer-events: none;
    }
    
    @keyframes ripple-animation {
        to {
            transform: scale(4);
            opacity: 0;
        }
    }
    
    /* تحسين حقول الإدخال */
    .form-group.focused .form-control,
    .form-group.focused .form-select {
        border-color: #C9A227;
        box-shadow: 0 0 0 0.2rem rgba(201, 162, 39, 0.25);
    }
    
    .form-group.has-value .form-label {
        color: #C9A227;
        font-weight: 600;
    }
    
    /* زر العودة للأعلى */
    .back-to-top {
        position: fixed;
        bottom: 20px;
        right: 20px;
        width: 50px;
        height: 50px;
        background: linear-gradient(135deg, #C9A227, #E8D48B);
        color: white;
        border: none;
        border-radius: 50%;
        font-size: 18px;
        cursor: pointer;
        opacity: 0;
        visibility: hidden;
        transition: all 0.3s ease;
        z-index: 1000;
        box-shadow: 0 4px 12px rgba(201, 162, 39, 0.3);
    }
    
    .back-to-top.show {
        opacity: 1;
        visibility: visible;
    }
    
    .back-to-top:hover {
        transform: translateY(-2px);
        box-shadow: 0 6px 16px rgba(201, 162, 39, 0.4);
    }
    
    /* شريط التقدم */
    .reading-progress {
        position: fixed;
        top: 0;
        left: 0;
        width: 0%;
        height: 3px;
        background: linear-gradient(90deg, #C9A227, #E8D48B);
        z-index: 9999;
        transition: width 0.3s ease;
    }
    
    /* تحسين شريط التنقل عند التمرير */
    .navbar-scrolled {
        background: rgba(255, 255, 255, 0.95) !important;
        backdrop-filter: blur(10px);
        box-shadow: 0 2px 20px rgba(0, 0, 0, 0.1);
    }
    
    /* تحسين اللمس */
    .touch-device .touch-active {
        transform: scale(0.98);
        opacity: 0.8;
    }
    
    /* تحميل تدريجي */
    .lazy-load {
        opacity: 0;
        transform: translateY(20px);
        transition: all 0.6s ease;
    }
    
    .lazy-load.loaded {
        opacity: 1;
        transform: translateY(0);
    }
    
    /* تحسين الصور */
    img.lazy {
        opacity: 0;
        transition: opacity 0.3s;
    }
    
    img.lazy.loaded {
        opacity: 1;
    }
    
    /* تحسينات للموبايل */
    @media (max-width: 768px) {
        .back-to-top {
            bottom: 15px;
            right: 15px;
            width: 45px;
            height: 45px;
            font-size: 16px;
        }
        
        .ripple {
            background: rgba(255, 255, 255, 0.4);
        }
    }
`;

document.head.appendChild(style);