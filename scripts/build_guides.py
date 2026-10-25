#!/usr/bin/env python3
"""Build static, source-linked DGII guides from the reviewed local content."""
from __future__ import annotations

import json
import re
from collections import defaultdict
from html import escape
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
SITE = ROOT / "site"
BASE = "https://sellernet.fcastro.site"
STAMP = "2026-10-24"
ARTICLES = json.loads((SITE / "content/guides.json").read_text(encoding="utf-8"))
SOURCES = json.loads((SITE / "content/dgii-sources.json").read_text(encoding="utf-8"))
GROUPS = ["Fundamentos", "XML y firma", "Servicios DGII", "Documentos especiales", "Operación"]


def inline(value: str) -> str:
    return re.sub(r"`([^`]+)`", lambda m: f"<code>{escape(m.group(1))}</code>", escape(value))


def shell(title: str, description: str, route: str, body: str, schema: dict) -> str:
    url = BASE + route
    ld = json.dumps(schema, ensure_ascii=False, separators=(",", ":")).replace("</", "<\\/")
    return f'''<!doctype html>
<html lang="es">
<head>
<meta charset="utf-8">
<meta name="viewport" content="width=device-width, initial-scale=1">
<title>{escape(title)} · SellerNet</title>
<meta name="description" content="{escape(description, quote=True)}">
<link rel="canonical" href="{url}">
<meta name="robots" content="index, follow, max-image-preview:large">
<meta property="og:type" content="{'website' if route == '/guias' else 'article'}">
<meta property="og:site_name" content="SellerNet">
<meta property="og:title" content="{escape(title, quote=True)}">
<meta property="og:description" content="{escape(description, quote=True)}">
<meta property="og:url" content="{url}">
<meta property="og:image" content="{BASE}/og-image.png">
<meta property="og:image:width" content="1200">
<meta property="og:image:height" content="630">
<meta name="twitter:card" content="summary_large_image">
<link rel="icon" href="/favicon.svg" type="image/svg+xml">
<link rel="preconnect" href="https://fonts.googleapis.com">
<link rel="preconnect" href="https://fonts.gstatic.com" crossorigin>
<link href="https://fonts.googleapis.com/css2?family=Space+Grotesk:wght@400;500;700&family=JetBrains+Mono:wght@400;600&display=swap" rel="stylesheet">
<link rel="stylesheet" href="/styles.css">
<link rel="stylesheet" href="/guides.css">
<script type="application/ld+json">{ld}</script>
</head>
<body>
<header><div class="frame"><nav class="nav" aria-label="Principal">
<a href="/" class="logo" aria-label="SellerNet, inicio"><img src="/logo.svg" alt="" width="27" height="34"><span>Seller<em>Net</em></span></a>
<ul><li><a href="/">Inicio</a></li><li><a href="/docs">Documentación</a></li><li><a href="/guias" aria-current="page">Guías DGII</a></li></ul>
<div class="nav-cta"><a class="btn-ghost" href="https://github.com/dev-fcastro/dgii-ecf" target="_blank" rel="noopener">GitHub</a><a class="btn" href="https://www.nuget.org/packages/DgiiEcf" target="_blank" rel="noopener">NuGet</a></div>
<button class="burger" aria-label="Menú" aria-expanded="false" aria-controls="mmenu"><span></span><span></span></button>
</nav></div><div class="h-rule"></div><div class="mobile-menu" id="mmenu"><a href="/">Inicio</a><a href="/docs">Documentación</a><a href="/guias" aria-current="page">Guías DGII</a><a href="https://www.nuget.org/packages/DgiiEcf" target="_blank" rel="noopener">NuGet</a></div></header>
<main>{body}</main>
<footer><div class="frame"><div class="foot-top"><span>© 2026 <a href="https://www.fcastro.site" target="_blank" rel="noopener">fcastro</a> · SellerNet · MIT</span><button type="button" id="totop">Volver arriba ↑</button></div></div><div class="h-rule"></div><div class="frame"><p class="guide-footer-note">Guías independientes basadas en documentos oficiales de la DGII. <a href="https://dgii.gov.do/cicloContribuyente/facturacion/comprobantesFiscalesElectronicosE-CF/Paginas/documentacionSobreE-CF.aspx" target="_blank" rel="noopener">Consulta la documentación vigente de la DGII ↗</a></p></div></footer>
<script src="/site.js" defer></script>
</body>
</html>
'''


def render_block(block: dict) -> str:
    kind = block["type"]
    if kind == "p":
        return f"<p>{inline(block['text'])}</p>"
    if kind == "note":
        return f"<aside class=\"guide-note\"><strong>Clave técnica</strong><p>{inline(block['text'])}</p></aside>"
    if kind == "list":
        return "<ul>" + "".join(f"<li>{inline(x)}</li>" for x in block["items"]) + "</ul>"
    if kind == "table":
        head = "".join(f"<th scope=\"col\">{escape(x)}</th>" for x in block["headers"])
        rows = "".join("<tr>" + "".join(f"<td>{inline(x)}</td>" for x in row) + "</tr>" for row in block["rows"])
        return f"<div class=\"guide-table-wrap\"><table><thead><tr>{head}</tr></thead><tbody>{rows}</tbody></table></div>"
    raise ValueError(kind)


def card(article: dict) -> str:
    return f'''<a class="guide-card" href="/guias/{article['slug']}"><span class="guide-card-kind">{escape(article['category'])}</span><h3>{escape(article['title'])}</h3><p>{escape(article['description'])}</p><span class="guide-card-link">Leer guía ↗</span></a>'''


def article_page(article: dict, index: int) -> str:
    slug = article["slug"]
    route = "/guias/" + slug
    sections = []
    toc = []
    for i, section in enumerate(article["sections"], 1):
        anchor = f"seccion-{i}"
        toc.append(f'<a href="#{anchor}">{escape(section["title"])}</a>')
        sections.append(f'<section id="{anchor}"><h2>{escape(section["title"])}</h2>' + "".join(render_block(b) for b in section["blocks"]) + "</section>")
    source_items = []
    citations = []
    for ref in article["sources"]:
        source = SOURCES[ref["id"]]
        url = source["url"] + ("#page=" + ref["pages"].split("–")[0].split(",")[0] if ref.get("pages") else "")
        citations.append(source["url"])
        page = f' · p. {escape(ref["pages"])}' if ref.get("pages") else ""
        source_items.append(f'<li><a href="{escape(url, quote=True)}" target="_blank" rel="noopener">{escape(source["title"])}{page} ↗</a></li>')
    related = [ARTICLES[(index + offset) % len(ARTICLES)] for offset in (1, 2, 3)]
    body = f'''<div class="frame"><div class="guide-breadcrumb"><a href="/">Inicio</a><span>/</span><a href="/guias">Guías DGII</a><span>/</span><span>{escape(article['category'])}</span></div></div>
<div class="h-rule"></div><div class="frame"><div class="guide-article-head"><span class="guide-eyebrow">{escape(article['category'])} · Guía {index+1:02d}</span><h1>{escape(article['title'])}</h1><p>{escape(article['lead'])}</p></div></div><div class="h-rule"></div>
<div class="frame"><div class="guide-article-layout"><aside class="guide-toc"><strong>En esta guía</strong>{''.join(toc)}<a href="#fuentes">Fuentes DGII</a></aside><article class="guide-article-body">{''.join(sections)}<section id="fuentes" class="guide-sources"><h2>Fuentes DGII</h2><p>Documentos del archivo oficial descargado el 22 de octubre de 2026. Consulta la versión vigente antes de implementar cambios.</p><ul>{''.join(source_items)}</ul></section></article></div></div>
<div class="h-rule"></div><section class="frame guide-related"><div class="guide-related-head"><h2>Continúa aprendiendo</h2><a href="/guias">Ver todas las guías ↗</a></div><div class="guide-grid">{''.join(card(a) for a in related)}</div></section>'''
    schema = {"@context":"https://schema.org","@graph":[{"@type":"Article","headline":article["title"],"description":article["description"],"inLanguage":"es-DO","mainEntityOfPage":BASE+route,"author":{"@type":"Person","name":"Francisco Daniel Castro Borrome","url":"https://www.fcastro.site/"},"publisher":{"@type":"Person","name":"Francisco Daniel Castro Borrome"},"citation":citations},{"@type":"BreadcrumbList","itemListElement":[{"@type":"ListItem","position":1,"name":"Inicio","item":BASE+"/"},{"@type":"ListItem","position":2,"name":"Guías DGII","item":BASE+"/guias"},{"@type":"ListItem","position":3,"name":article["title"],"item":BASE+route}]}]}
    return shell(article["title"],article["description"],route,body,schema)


def hub_page() -> str:
    grouped = defaultdict(list)
    for a in ARTICLES:
        grouped[a["category"]].append(a)
    groups = "".join(f'<section class="guide-hub-group" id="{re.sub(r"[^a-z]+", "-", category.lower()).strip("-")}"><div class="guide-group-head"><h2>{escape(category)}</h2><span>{len(grouped[category])} guías</span></div><div class="guide-grid">{"".join(card(a) for a in grouped[category])}</div></section>' for category in GROUPS)
    body = f'''<div class="frame"><div class="guide-hub-hero"><span class="guide-eyebrow">Base de conocimiento · e-CF · República Dominicana</span><h1>Entiende la facturación electrónica de la DGII.</h1><p>De los conceptos al XML, la firma y los servicios. Once guías técnicas basadas en los formatos, XSD e instructivos oficiales incluidos en la documentación de la DGII.</p><a class="btn" href="#fundamentos">Explorar las guías ↓</a></div></div><div class="h-rule"></div><div class="frame guide-hub-body"><div class="guide-source-callout"><strong>Fuente, no suposición.</strong><p>Revisamos los documentos oficiales del archivo DGII descargado el 22 de octubre de 2026. Cada guía enlaza los PDF o XSD que respaldan sus afirmaciones.</p><a href="https://dgii.gov.do/cicloContribuyente/facturacion/comprobantesFiscalesElectronicosE-CF/Paginas/documentacionSobreE-CF.aspx" target="_blank" rel="noopener">Documentación oficial DGII ↗</a></div>{groups}</div>'''
    schema = {"@context":"https://schema.org","@type":"CollectionPage","name":"Guías técnicas de facturación electrónica DGII","url":BASE+"/guias","inLanguage":"es-DO","hasPart":[{"@type":"Article","name":a["title"],"url":BASE+"/guias/"+a["slug"]} for a in ARTICLES]}
    return shell("Guías de facturación electrónica DGII","Aprende qué es un e-CF y cómo funcionan sus tipos, XML, firma, autenticación, envío, RFCE, receptor y certificación con fuentes oficiales.","/guias",body,schema)


def main() -> None:
    slugs=[a["slug"] for a in ARTICLES]
    assert len(slugs)==len(set(slugs))
    output=SITE/"guias"
    output.mkdir(exist_ok=True)
    (SITE/"guias.html").write_text(hub_page(),encoding="utf-8")
    for index,article in enumerate(ARTICLES):
        for ref in article["sources"]:
            assert ref["id"] in SOURCES,(article["slug"],ref["id"])
        (output/(article["slug"]+".html")).write_text(article_page(article,index),encoding="utf-8")
    urls=[("/", "1.0"),("/docs", "0.8"),("/guias", "0.9")]+[("/guias/"+slug,"0.7") for slug in slugs]
    site_map='''<?xml version="1.0" encoding="UTF-8"?>\n<urlset xmlns="http://www.sitemaps.org/schemas/sitemap/0.9">\n'''+''.join(f'  <url><loc>{BASE}{route}</loc><lastmod>{STAMP}</lastmod><changefreq>monthly</changefreq><priority>{priority}</priority></url>\n' for route,priority in urls)+'</urlset>\n'
    (SITE/"sitemap.xml").write_text(site_map,encoding="utf-8")
    print(f"Built hub and {len(ARTICLES)} articles")

if __name__ == "__main__":
    main()
