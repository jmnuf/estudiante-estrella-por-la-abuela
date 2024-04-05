# Reglas Generales de la Base de Codigo

1. Todos los variables seran en `snake_case`.
2. Se agruparan scripts y prefabs juntos bajo carpeta si son especificos del uno al otro.
3. Se agruparan scripts y "Scriptable Objects" bajo la misma carpeta si son especificos del uno al otro.
4. El uso de Singletons sera limitado y basado en estilo C#/Java a menos que ocupé habilidades de Componente en cual caso se utilizara el estilo de Singletons de Unity.
5. Si hacen faltan Assets y solo estan una carpeta vacia con un ".gitinclude" significa que el asset que va en esa carpeta se descargan desde la unity asset store o de alguna otra tienda con un enlace que ira dentro de dicho ".gitinclude"
6. Los operadores no se deben override, siempre una funcion por encima.
7. Propiedades calculadas son de un infierno al que no pertenzco, así que aqui no se aceptan
8. Comentar cosas que no sean obvias a la vista con lo minimo de contexto.

