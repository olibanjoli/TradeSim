# TradeSim

Bot to assist in trade simulations.

<img width="370" alt="image" src="https://github.com/olibanjoli/TradeSim/assets/1844103/14b7ce97-3e3a-4d2f-9e8a-2d16e803366e">



## Commands

### Start
Start at a specific price level

```/sim start <Price>```

### Pause
Before next tick, pause so no more orders are accepted from reactions

```/sim pause```

### Tick
Add value of next candle

```/sim tick <Price>```

### Scores
Show all scores

```/sim scores```

### Orders
Show all open orders

```/sim orders```

### Close
Close all orders with given price. Useful for last candle in simulation.

```/sim close <Price>```

### Set Score
Set current score of a user to a specific value. Any open orders will stay open.

```/sim set-score @user 1337```

### Remove Order
Remove an open order of a user

```/sim remove-order @user```

### Reset 2x
Each user can 2x once. To allow a user to 2x again:

```/sim reset-2x @user```

### Reset 2x all
Allow everyone to 2x again

```/sim reset-2x-all```

### Reset
Reset the simulation. After reset, start a new session using .start <Price>

```/sim reset```

### Set Tick Value
Set tick value for $ calculation in highscores list

```/sim tick-value <TickValue>```
