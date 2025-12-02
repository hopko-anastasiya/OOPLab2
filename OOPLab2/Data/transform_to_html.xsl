<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform">
	<xsl:output method="html" encoding="UTF-8" indent="yes"/>

	<xsl:template match="/archive">
		<html>
			<head>
				<title>
					Звіт: <xsl:value-of select="@name"/>
				</title>
				<style>
					body { font-family: Arial, sans-serif; margin: 20px; }
					table { border-collapse: collapse; width: 80%; }
					th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }
					th { background-color: #f2f2f2; }
				</style>
			</head>
			<body>
				<h1>
					Електронний Архів: <xsl:value-of select="@name"/>
				</h1>
				<p>
					Дата Створення: <b>
						<xsl:value-of select="@creationDate"/>
					</b>
				</p>

				<table>
					<tr>
						<th>Автор (Ф-т/Каф)</th>
						<th>Назва Матеріалу</th>
						<th>Вид</th>
						<th>Обсяг (стор.)</th>
						<th>Дата</th>
					</tr>
					<xsl:apply-templates select="material"/>
				</table>
			</body>
		</html>
	</xsl:template>

	<xsl:template match="material">
		<tr>
			<td>
				<xsl:value-of select="author"/>
				(<xsl:value-of select="author/@faculty"/> / <xsl:value-of select="author/@department"/>)
			</td>
			<td>
				<xsl:value-of select="title"/>
			</td>
			<td>
				<xsl:value-of select="@type"/>
			</td>
			<td>
				<xsl:value-of select="pages"/>
			</td>
			<td>
				<xsl:value-of select="date"/>
			</td>
		</tr>
	</xsl:template>

</xsl:stylesheet>