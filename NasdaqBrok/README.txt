El fichero NasdaqBrok.zip tiene la estructura de directorios y un ejemplo. 
Usar este si es la primera vez que se descarga. El fichero src.zip solo tiene el código fuente. 
Usar este si en algún momento se actualiza el código.

Si se escribe en el navegador de Internet https://www.nasdaq.com/api/v1/historical/optt/stocks/2020-01-21/2021-01-21 se descargan los datos históricos de OPTT (Ocean Power Technologies) del servidor de NASDAQ en el intervalo de fechas que se indica después. He probado a ponerlo directamente en una hoja de cálculo pero no funciona; así que he hecho un programa en C# 

El código está en la carpeta src. Para compilarlo con Visual Studio simplemente abrir el fichero de proyecto .sln Asegurarse que está incluida la librería System.Net.Http. No está incluida por defecto. Para compilarlo con Mono no debería haber ningún problema, pero no lo he probado.

El programa busca en el directorio /data ficheros con el nombre del símbolo terminado en .csv; por ejemplo optt.csv Actualizará los datos desde la última fecha almacenada o si el fichero está vacío se descargará los dos últimos años. Luego en el directorio /ods tengo las hojas de cálculo desde donde se importa el fichero .csv dentro del directorio /data.

Para importar el fichero .csv en la hoja de cálculo en Libre Office: 
	Pinchar en la casilla A1 (yo lo tengo en la carpeta linked_data) Menu -> Sheet -> Link to External Data 
	Seleccionar el fichero .csv en la carpeta /data 
	En idioma indicar English para que coja el . como separador decimal. 
	La tabla se actualizará con los nuevos datos cada vez que abrimos el fichero por lo que solo hace falta hacerlo la primera vez.

Con Excel será un proceso algo parecido. No tengo licencia así que no he podido intentarlo y hace tiempo que no lo uso. El formato .ods debería reconocerlo Excel por lo que debería funcionar el ejemplo.

Los datos que se importan aparece la última fecha la primera, lo que no lo hace muy conveniente para los gráficos. Dentro de la hoja de cálculo hago un offset para invertir el orden de las columnas.
