(() => {
  // install tabs + copy (landing)
  const install = document.getElementById('install');
  if (install) {
    const cmdEl = install.querySelector('.cmd'), prefixEl = install.querySelector('.p');
    install.querySelectorAll('[role=tab]').forEach(tab => tab.addEventListener('click', () => {
      install.querySelectorAll('[role=tab]').forEach(t => t.setAttribute('aria-selected', t === tab));
      cmdEl.textContent = tab.dataset.cmd;
      prefixEl.textContent = tab.dataset.prefix ? tab.dataset.prefix + ' ' : '';
    }));
    const copy = install.querySelector('.copy');
    copy.addEventListener('click', async () => {
      try { await navigator.clipboard.writeText(cmdEl.textContent); copy.textContent = 'Copiado'; }
      catch { copy.textContent = 'Error'; }
      setTimeout(() => copy.textContent = 'Copiar', 1600);
    });
  }

  // roles (landing)
  const tagsEl = document.getElementById('tags');
  if (tagsEl) {
    const roles = {
      emisor:   { file: 'FacturacionService.cs', tags: ['Autenticación por semilla', 'Token auto-renovado', 'Envío de e-CF', 'Aprobación comercial', 'Anulación de secuencias', 'Consultas de estado'] },
      receptor: { file: 'Program.cs', tags: ['Semilla', 'Validación de semilla firmada', 'Emisión de JWT', 'Lectura de multipart', 'Acuse de recibo ARECF', 'Rechazos automáticos'] },
      firma:    { file: 'Firma.cs', tags: ['XML-DSig enveloped', 'C14N inclusivo', 'RSA-SHA256', 'Certificado .p12 o base64', 'Documentos tipados'] },
      tools:    { file: 'Consumo.cs', tags: ['Código de seguridad', 'e-CF 32 → RFCE', 'URL del QR', 'JSON ⇄ XML', 'Compatible con dgii-ecf Node'] }
    };
    const fname = document.getElementById('fname');
    const roleBtns = document.querySelectorAll('.roles button');
    function show(role) {
      roleBtns.forEach(b => b.setAttribute('aria-selected', b.dataset.role === role));
      document.querySelectorAll('.code-pane').forEach(p => p.classList.toggle('on', p.dataset.pane === role));
      fname.textContent = roles[role].file;
      tagsEl.innerHTML = roles[role].tags.map((t, i) => `<span style="animation-delay:${i * 70}ms">${t}</span>`).join('');
    }
    roleBtns.forEach(b => { b.addEventListener('mouseenter', () => show(b.dataset.role)); b.addEventListener('click', () => show(b.dataset.role)); });
    show('emisor');
  }

  // reveal + counters
  const io = new IntersectionObserver(entries => entries.forEach(e => {
    if (!e.isIntersecting) return;
    e.target.classList.add('in');
    if (e.target.dataset.count) {
      const end = +e.target.dataset.count; let n = 0;
      const step = () => { e.target.textContent = ++n; if (n < end) setTimeout(step, 140); };
      step();
    }
    io.unobserve(e.target);
  }), { threshold: .3 });
  document.querySelectorAll('.reveal,[data-count]').forEach(el => io.observe(el));

  // mobile menu
  const burger = document.querySelector('.burger'), menu = document.getElementById('mmenu');
  burger.addEventListener('click', () => { const open = menu.classList.toggle('open'); burger.setAttribute('aria-expanded', open); });
  menu.querySelectorAll('a').forEach(a => a.addEventListener('click', () => { menu.classList.remove('open'); burger.setAttribute('aria-expanded', false); }));

  // docs scrollspy
  const docLinks = [...document.querySelectorAll('.docs-nav a')];
  const spy = new IntersectionObserver(entries => entries.forEach(e => {
    if (!e.isIntersecting) return;
    docLinks.forEach(a => {
      const on = a.getAttribute('href') === '#' + e.target.id;
      a.classList.toggle('on', on);
      if (on && a.parentElement.scrollWidth > a.parentElement.clientWidth)
        a.parentElement.scrollTo({ left: a.offsetLeft - 24, behavior: 'smooth' });
    });
  }), { rootMargin: '-30% 0px -60% 0px' });
  document.querySelectorAll('.doc').forEach(d => spy.observe(d));

  document.getElementById('totop').addEventListener('click', () => scrollTo({ top: 0, behavior: 'smooth' }));
})();
