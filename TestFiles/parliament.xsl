<xsl:stylesheet xmlns:xsl="http://www.w3.org/1999/XSL/Transform" version="1.0">
	<xsl:output method="html" encoding="utf-8" indent="yes"/>
	<xsl:template match="/">
		<html>
			<body>
				<h2>Студентський парламент</h2>
				<p>
					Кількість записів:
					<xsl:value-of select="count(//Event)"/>
				</p>
				<xsl:if test="count(//Event) = 0">
					<p>Нічого не знайдено.</p>
				</xsl:if>
				<xsl:if test="count(//Event) > 0">
					<table border="1" cellpadding="5" cellspacing="0">
						<tr>
							<th>№</th>
							<th>П.І.П.</th>
							<th>Факультет</th>
							<th>Кафедра</th>
							<th>Спеціальність</th>
							<th>Вид заходу</th>
							<th>Час</th>
							<th>Опис</th>
						</tr>
						<xsl:for-each select="//Event">
							<xsl:sort select="@FullName"/>
							<tr>
								<td>
									<xsl:value-of select="position()"/>
								</td>
								<td>
									<xsl:value-of select="@FullName"/>
								</td>
								<td>
									<xsl:value-of select="@Faculty"/>
								</td>
								<td>
									<xsl:value-of select="@Department"/>
								</td>
								<td>
									<xsl:value-of select="@Specialty"/>
								</td>
								<td>
									<xsl:value-of select="@EventType"/>
								</td>
								<td>
									<xsl:value-of select="@TimeFrame"/>
								</td>
								<td>
									<xsl:value-of select="Description"/>
								</td>
							</tr>
						</xsl:for-each>
					</table>
				</xsl:if>
			</body>
		</html>
	</xsl:template>
</xsl:stylesheet>