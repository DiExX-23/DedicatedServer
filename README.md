# Servidor Dedicado
Este proyecto permite la conexión multijugador de un videojuego en tiempo real.

El proyecto funciona gracias a una API de Docker que recibe y envía posiciones. Docker es el servidor que maneja los datos de los jugadores que estén conectados. Cada jugador tiene un ID y un ID de partida en la que se encuentra, esos IDs se envían al servidor junto con las posiciones de cada jugador. Según las posiciones que van llegando, se generan las copias de los jugadores en cada una de las pantallas. Las solicitudes de envío y recivimiento de los datos se hacen a tiempo real.

El videojuego consiste en un laberinto en donde cada jugador deberá buscar el camino para llegar a la meta, el primero en llegar se hará con la victoria. Cada partida puede contener hasta 4 jugadores simultáneos.
