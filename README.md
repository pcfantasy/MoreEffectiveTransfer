# Inner workings

## Settings and their effect

### Prefer local district services (recommended to enable)
<b>Affects: garbage, police, health care, maintenance, mail, taxi</b><br />
This setting further improves locality of services by further narrows down the matchmaking:<br />
All low priority (priority 0..4) outgoing transfer requests will only match with services from the same district. If no matching services from the same district are available, the request is unfulfilled.
As the problem (garbage, crime, ...) accumulates, the request will gain urgency and increase priority. Once reaching priority classes 5..7 it will be matched to fulfill, regardless of district restrictions.
<br /><b>Effect:</b><br />
While not quite duplicating mods such as DistrictServiceLimit or EnhancedDistrictServices, it ensures that most services stay local as much as possible, while at the same time ensuring that high priority requests will be serviced, even if that means a service has to "rush across the map".
<br /><br />


### Warehouse First (recommended to enable)
<b>Affects: warehouse goods deliveries</b><br />
This setting makes all incoming and outgoing goods deliveries prefer warehouses as source/destination.
<br /><b>Effect:</b><br />
This can help a lot in city logistics if there are some well-placed warehouses as "buffers" between industry & commercial, or industrial producers & consumers.
Note that this can make things worse if you do not have strategically well-placed warehouses, and will in general increase traffic around warehouse routes.
<br /><br />


### Reserve Warehouse Trucks for local transfer (recommended to enable if Warehouse First is used)
<b>Affects: warehouse goods exports/b><br />
This setting will prevent all warehouses from using all their trucks to export goods out of the city.
A contigent of trucks will be held in reserve and will only be used to fulfill requests from consumers within the city.<br />
The setting's effect is: <br />
* 50% capacity reserved when warehouse is in FILLING or BALANCED mode.<br />
* 25% capacity reserved when warehouse is in EMPTYING mode.<br />
<br /><br />

  
## Problems
There is obviously a conflict between the "higher priority first" approach and the "closed distance preferred" approach.
In particular, preferring matchmaking by closest distance cannot fully prevent that buildings get serviced by service buildings from far away.
That is because a more suitable, closer service offer might have already been matched to a higher priority request, so now the closer building has no choice but be matched with an offer from a service further away.

This whole situation cannot be easily prevented, and would rather require a complete redesign of the offer matchmaking system, with a "global optimization" approach (instead of the current "line-by-line, from high to low priority" approach).

TLDR: you notice in your city that garbage trucks go from A to B, and from B to A, and you think this mod is not doing its' job. C'est la vie - this mod usually reduces these situations a lot, but cannot fully prevent them.
<br /><br />


# For reference also see original user guide / wiki by pcfantasy:
#### MoreEffectiveTransfer User Guide [![Steam Downloads](https://img.shields.io/steam/downloads/1680840913.svg?label=Steam%20downloads&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=1680840913)
[English version](https://github.com/pcfantasy/MoreEffectiveTransfer/wiki/English-UG) <br>
#### MoreEffectiveTransfer 说明书
[MoreEffectiveTransfer 模组说明书](https://github.com/pcfantasy/MoreEffectiveTransfer/wiki/%E4%B8%AD%E6%96%87%E8%AF%B4%E6%98%8E%E4%B9%A6) <br>
