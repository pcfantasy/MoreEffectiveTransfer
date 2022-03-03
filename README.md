# Inner workings
This is a continuation of <b><i>pcfantasy's</i> More Effective Transfer Manager</b>
 (https://steamcommunity.com/sharedfiles/filedetails/?id=1680840913).
<br /><br />
He was so kind as to license his code under the MIT license for us, so if you like this mod, please take the time to go to the above posting and show your appreciation for his original work!
<br /><br />
The core code for the match-making logic has been mostly completely rewritten, but with pcfantasy's original idea and design in mind.
<br /><br />

![This mod is compatible with the Airports DLC and the accompanying base game patch](https://i.imgur.com/NWy2c0D.png)

## What it does
The vanilla transfer managers handles service requests and goods transfers by matching highest priority orders (incoming and outgoing) first, regardless of distance.
The observed effect is that trucks go from A to B, and from B to A; fire response is inefficient if fire fighters have to rush through heavy traffic across the map, and problems such as garbage or crime are unattended for longer than necessary.
<br/>
You can learn more about the inner workings of the vanilla transfer manager by reading the excellent article here: https://jamesmonger.com/2021/02/24/cities-skylines-trading-market.html
<br /><br />
This mod attempts to improve the situation by:
<ul>
<li>Optimizes the service dispatching and goods transfer for industries. All match-making between supply & demand is done by shortest distance ("as the crow flies", not actual pathfinding distance!).
<li>Serves highest priority requests (=more urgent problems) first, working its way down the queue (like vanilla).
<li>Different match-making modes carefully selected per service / goods: OUTGOING_FIRST (most services), INCOMING FIRST (transfers and maintenance), and BALANCED (goods).
<li>Additional options to further improve service locality and warehouse effectiveness. If they work as intended heavily depends on how your city is structured, though. YMMV.
</ul>

## What it does not
It does not restrict any citizen activities: citizens go to any school as they like, they go shopping wherever they please, they pick a work place as they want, and they dont just go to the nearby park, instead travelling all across the city to the fancy 5* zoo.<br/>
This is intentional - the Cims value their freedom and certainly dont accept being restricted to their district...
<br /><br />
This mod does not change the match-making to use actual pathfinding distance/routing. Using that would be very costly without optimized data structures and algorithms, and cannot be easily done. 


## Settings and their effect

### Prefer local district services (recommended to enable)
<b>Affects: garbage, police, health care, maintenance, mail, taxi</b><br />
This setting further improves locality of services by further restricting the matchmaking:<br />
All low priority outgoing transfer requests will only match with services from the same district. If no matching services from the same district are available, the request is unfulfilled.
As the problem (garbage, crime, ...) accumulates, the request will gain urgency and increase priority. Once reaching hiher priority it will be matched to fulfill, regardless of district restrictions (but still with a bias towards local district services).
<br /><b>Effect:</b><br />
While not quite duplicating mods such as DistrictServiceLimit or EnhancedDistrictServices, it ensures that most services stay local as much as possible, while at the same time ensuring that high priority requests will eventually be serviced, even if that means a service has to "rush across the map".
<br /><br />


### Warehouse First (recommended to enable)
<b>Affects: warehouse goods deliveries</b><br />
This setting makes all incoming and outgoing goods deliveries prefer warehouses as source/destination.
<br /><b>Effect:</b><br />
This can help a lot in city logistics if there are some well-placed warehouses as "buffers" between industry & commercial, or industrial producers & consumers.
Note that this can make things worse if you do not have strategically well-placed warehouses, and will in general increase traffic around warehouse routes.
<br /><br />


### Reserve Warehouse Trucks for local transfer (recommended to enable if Warehouse First is used)
<b>Affects: warehouse goods exports</b><br />
This setting will prevent all warehouses from using all their trucks to export goods out of the city.
A contigent of trucks will be held in reserve and will only be used to fulfill requests from consumers within the city.<br />
The setting's effect is: <br />
* 25% capacity reserved at all times.<br />
<br /><br />

  
## Problems
There is obviously a conflict between the "higher priority first" approach and the "closed distance preferred" approach.
In particular, preferring matchmaking by closest distance cannot fully prevent that buildings get serviced by service buildings from far away.
That is because a more suitable, closer service offer might have already been matched to a higher priority request, so now the closer building has no choice but be matched with an offer from a service further away.

This whole situation cannot be easily prevented, and would rather require a complete redesign of the offer matchmaking system, with a "global optimization" approach (instead of the current "line-by-line, from high to low priority" approach).

TLDR: you notice in your city that garbage trucks go from A to B, and from B to A, and you think this mod is not doing its' job. This mod usually reduces these situations, but cannot fully prevent them.
<br /><br />


# For reference also see original user guide / wiki by pcfantasy:
#### MoreEffectiveTransfer User Guide [![Steam Downloads](https://img.shields.io/steam/downloads/1680840913.svg?label=Steam%20downloads&logo=steam)](https://steamcommunity.com/sharedfiles/filedetails/?id=1680840913)
[English version](https://github.com/pcfantasy/MoreEffectiveTransfer/wiki/English-UG) <br>
#### MoreEffectiveTransfer 说明书
[MoreEffectiveTransfer 模组说明书](https://github.com/pcfantasy/MoreEffectiveTransfer/wiki/%E4%B8%AD%E6%96%87%E8%AF%B4%E6%98%8E%E4%B9%A6) <br>

# Impact and Performance
As of version 2.1.0.220303 the match-making is run on a background thread, reducing the burden for the game's main simulation thread.
There is some performance improvement over vanilla due to this, although most likely this wont translate to higher frame rate.
<br />
Vanilla transfer manager on a large city: 0.6ms time per run
<br />
METM on the same city: ~0.45ms time per run
<br />

# Source Code
On Github: https://github.com/TheDogKSP/MoreEffectiveTransfer
<br />
License: MIT
<br />
Be sure to always respect pcfantasy's original copyright!
<br />
This mod uses the Harmony2 library by Andreas Pardeike.

# Incompatible mods:
(1) Original More Effective Transfer Manager by pcfantasy. Please completely unsubscribe original before using this one.<br/>
(2) District Service Limit (any version)<br/>
(3) Enhanced District Services <br/>
(4) Transfer Broker - MALWARE WARNING! DO NOT USE!
<br/>

# Recommended mods:
(1) I highly recommend to use this mod in conjunction with <b>"Smarter Firefighters: Improved AI"</b> by themonthlydaily (https://steamcommunity.com/sharedfiles/filedetails/?id=2346565561).
<br/>
While METM ensures that your fires are serviced by the cloeset fire department, Smarter Firefighters will make each fire truck (or copter) vehicle work relentlessly until everything is extinguished before calling it a day and returning to the station.
With both mods together your city fire fighting force will actually be quite effective in containing and putting out fire spread.

# Contributions wanted
Contributions in the form of Github Pull Requests are welcome!
Unfortunately with the rewrite of this mod the original localizations have been lost. If you want to provide a localization to any language, this is always welcome!

