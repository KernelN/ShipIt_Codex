# ShipIt

Mobile 3D game. Made in Unity 6.0
Will use Facade, Composite, Factory, Builder and Memento design patterns.
Make use of the UpdateManager whenever possible to minimize CPU load. UpdateUserExample shows how the UpdateManager should be used.

V1
The ship must advance toward its destination (forward).
The ship falls onto a planet that spins in a random direction.
Facade to get critical data about the planet (for now, just the landing distance). 
Composite to give AstralBodies behaviour (for now, making planets rotate on axis)
It has to take off when it points to a planet ahead (indicate when the ship can hit and when it is pointing outside the target). (quick tap)
If you shoot the ship into space, you see it go and respawn. You lose points (future currency).
Using touch, you can rotate around the planet to search for the target. (long press)
You have a fuel pool. Each unit of movement costs you fuel. Each takeoff consumes fuel. You reload fuel every so often.
The less time you take, the more currency you earn (tip). The longer you take after the tip time, the less you earn. You lose earnings more slowly with the penalty than with the loss of tip (perhaps in chunks?).
Memento to save fuel and tips (save player data in general).
PUFFLE LAUNCH, BUT 3D

V2
There is now a map of where you have to go, which you see before takeoff. (proc gen of the planets on the trajectory) 
Hazardous planets are added. Through Facade, ship triggers a ShipEnteredBody event, and all components which are suscribed (hazard component for now) trigger. Hazard stores ship until a ShipLeftBody is sent, in the meantime, deals dmg every certain time.
Factory to instantiate AstralBodies. Manager requests amount of bodies. Factory has to define size, axis and components for each of them. Factory uses SO to determine proc gen stuff. 
The destination is a specific planet, you have the path marked. (marked in gold)
You can make a custom path (marked in blue).
Instead of manually pointing towards the next planet, the ship automatically goes towards the planet on the custom path as soon as you tap.
Quick time event to enter orbit instead of landing (quick tap + UI). Entering and leaving orbit does not consume fuel. The better you do at the quick time event the less fuel you consume (0 if it's perfect). The ship also moves quicker the better you do at the event.

V3
The map is semi-permanent and gets bigger (with upgrades?). It is still procgen, but once it is generated, it is done.
The generated map is added to Memento.
You have different contracts/orders on the map (each on a different planet). They regenerate every X amount of time and each lasts for a different amount of time. Contracts are semi-random. Each contract has a different reward (more or less currency, sometimes fuel).
