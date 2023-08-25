# MultithreadedVectorFields

Experimental **FNA** project for generating agent vector fields in a **multithreaded** manner - with **no runtime heap allocations**

All of the traditional .Net means of starting a multithreaded process allocate somewhere in the Framework:
- **ThreadPool.QueueUserWorkItem**
- **Task.Run**
- **Task.Factory.StartNew**

_You may be asking yourself, are these allocations _really_ a problem?_

Depending on your game, the rate at which you alocate heap memeory, your target systems, this indeed may not be an issue for you. The allocations are small and short lived, resulting in fast, Gen 0 collections. 

However, realtime systems, especially games, are sensitive to garbage collection stalls and many developers take a policy of '**zero allocations during gameplay**'. Your code may need to run on lower powered devices like mobile phones or handhelp consoles and it's here where you'll really feel the benefit of fewer heap allocations.

> _Fun fact - the custom version of the compact .Net framework that the Xbox 360 used for XNA would run a full (Gen 2) garbage collection 
each time 1mb of memory was ALLOCTED (yeah, that's right, allocated! So hang onto those object references all you like, the GC would 
still run!)_

### Custom Threadpool Component

A custom threadpool component was used to circumvent .Net threading allocations. This component creates a series of threads at application startup, one per core, and assigns worker processes to each one. The application can add tasks to these workers as required, either to a specific worker (e.g. a worker the application has specifically reserved for audio or agent pathfinding maybe) or the next worker in line.

### ECS 

This demo uses the excellent ECS by [MoonTools.ECS](https://gitea.moonside.games/MoonsideGames/MoonTools.ECS).

> Entity component system (ECS) is a software architectural pattern mostly used in video game development for the representation of game world objects. An ECS comprises entities composed from components of data, with systems which operate on entities' components.
> 
> ECS follows the principle of composition over inheritance, meaning that every entity is defined not by a type hierarchy, but by the components that are associated with it. Systems act globally over all entities which have the required components. 

It could be considered a 'Pure' ECS:
- [E]ntity - Nothing more than a number - acts as an 'indexer' into the various component collections.
- [C]omponent - Data, no behaviour - components in MoonTools.ECS are limited to unmanaged values types only, no class references are allowed here.
- [S]ystem - Functions that operate on entities that conform to a certain set of components
  
  > e.g. A system to move entities in the world could query for entities with both Position and Velocity components.
  
There a two systems of interest here: 
- **CreateVectorFieldSystem** - posts requests for entity vector field creation to the CalculateVectorFieldsJob class.
- **ConsumeVectorFieldSystem** - consumes the results of the vector field calculation from the CalculateVectorFieldsJob class.

Simple BlockingCollection<T> containers are used to manage accessing shared data efficiently from multiple threads.

VectorFields objects are also pooled to further eliminate gameplay allocations.

## Credits

Inspiration:
- ThreadPoolComponent by [Jon Watte](http://www.enchantedage.com/)

Frameworks:
- [FNA](https://github.com/FNA-XNA/FNA) - _an XNA4 reimplementation that focuses solely on developing a fully accurate XNA4 runtime for the desktop._
- [MoonTools.ECS](https://gitea.moonside.games/MoonsideGames/MoonTools.ECS) _by MoonsideGames_
