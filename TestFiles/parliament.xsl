<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
  <xsl:output method="html" encoding="utf-8" indent="yes"/>

  <xsl:template match="/">
    <html>
      <head>
        <title>Студентський парламент</title>
        <style>
          body { font-family: Segoe UI, Arial, sans-serif; margin: 20px; color: #222; }
          h1 { margin-bottom: 4px; }
          .sum { color: #555; margin-bottom: 14px; }
          table { border-collapse: collapse; width: 100%; }
          th, td { border: 1px solid #bbb; padding: 6px 8px; vertical-align: top; text-align: left; }
          th { background: #2d5da8; color: #fff; }
          tr:nth-child(even) td { background: #f3f6fb; }
          .muted { color: #777; font-size: 12px; }
          .empty { padding: 20px; background: #fff4e5; border: 1px solid #f0c78a; }
        </style>
      </head>
      <body>
        <h1>Студентський парламент</h1>
        <div class="sum">Кількість записів: <b><xsl:value-of select="count(//Event)"/></b></div>
        <xsl:choose>
          <xsl:when test="count(//Event) = 0">
            <div class="empty">За заданими критеріями нічого не знайдено.</div>
          </xsl:when>
          <xsl:otherwise>
            <table>
              <tr>
                <th>№</th><th>П.І.П.</th><th>Факультет</th><th>Кафедра</th><th>Спеціальність</th>
                <th>Захід</th><th>Час проведення</th><th>Додатково</th>
              </tr>
              <xsl:apply-templates select="//Event">
                <xsl:sort select="@FullName" lang="uk"/>
              </xsl:apply-templates>
            </table>
          </xsl:otherwise>
        </xsl:choose>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="Event">
    <tr>
      <td><xsl:value-of select="position()"/></td>
      <td><b><xsl:value-of select="@FullName"/></b></td>
      <td><xsl:value-of select="@Faculty"/></td>
      <td><xsl:value-of select="@Department"/></td>
      <td><xsl:value-of select="@Specialty"/></td>
      <td>
        <b><xsl:value-of select="@EventType"/></b>
        <xsl:if test="Description"><br/><xsl:value-of select="Description"/></xsl:if>
      </td>
      <td>
        <xsl:choose>
          <xsl:when test="contains(@TimeFrame,'/')">
            <xsl:value-of select="substring-before(@TimeFrame,'/')"/>
            <xsl:if test="substring-before(@TimeFrame,'/') != substring-after(@TimeFrame,'/')">
              <xsl:text> — </xsl:text>
              <xsl:value-of select="substring-after(@TimeFrame,'/')"/>
            </xsl:if>
          </xsl:when>
          <xsl:otherwise><xsl:value-of select="@TimeFrame"/></xsl:otherwise>
        </xsl:choose>
      </td>
      <td>
        <xsl:if test="@Role">Роль: <xsl:value-of select="@Role"/><br/></xsl:if>
        <xsl:if test="@Course">Курс: <xsl:value-of select="@Course"/><br/></xsl:if>
        <xsl:if test="@Email">E-mail: <xsl:value-of select="@Email"/><br/></xsl:if>
        <span class="muted">атрибутів у вузлі: <xsl:value-of select="count(@*)"/></span>
      </td>
    </tr>
  </xsl:template>
</xsl:stylesheet>
