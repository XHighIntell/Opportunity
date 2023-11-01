# Strategy Double 

#### Parameters
`QUANTITY` $$Q$$
`MIN_PRICE_CHANGE_PERCENT` $$C$$
`LOSS_SERIES_MIN_PRICE_CHANGE_MULTIPLIER` $$M_c$$
`ENTRY_DIFF_PERCENT` $$E$$
`LOSS_SERIES_ENTRY_DIFF_MULTIPLIER` $$M_e$$


# Calculate Current Base on Previous Price
$$Quantity = 
 \begin{cases}
    Q & \text{if } t \leq 2 \\
    Q * 2 ^ {t-2} & \text{otherwise} \\
\end{cases} ⇔ ∑ _{Quantity} =Q * 2^{t-1}$$

##### Buy Series Loss
$$P_t=P_{t-1} * (1-\frac{C}{100} * M_c^{t-1})*(1-\frac{E}{100} * M_e^{t-1})$$

$$P_{exit}=P_t * (1+\frac{C}{100} * M_c^{t-1})*(1+\frac{E}{100}*M_e^{t-1})$$

##### Sell Series Loss
$$P_t=P_{t-1} * (1+\frac{C}{100} * M_c^{t-1})*(1+\frac{E}{100} * M_e^{t-1})$$

$$P_{exit}=P_t*(1-\frac{C}{100} * M_c^{t-1})*(1-\frac{E}{100}*M_e^{t-1})$$

# The Equation Base On $$P_1$$
This equation is useful for calculating the price, change percent for final try.

##### Buy Series Loss
$$\text{Entry Price (BUY)} = P_t =P_1 * \prod_{n=1}^{t-1}((1 - \frac{C}{100} * {M_c}^n)) * (1-\frac{E}{100} * {M_e}^n))$$
$$\text{Exit Price (SELL)} = P_{exit} = P_t *(1+\frac{C}{100} * {M_c}^{t-1}) * (1-\frac{E}{100}*M_e^{t-1})$$

Use this excel https://github.com/XHighIntell/Opportunity/raw/master/Math.xlsx
Use this chart https://www.desmos.com/calculator/alvpfktjkg

##### Sell Series Loss
$$\text{Entry Price (SELL)} = P_t = P_1 *  \prod_{n=1}^{t-1} \left(1 + \frac{C}{100} * {M_c}^n \right)$$
$$\text{Exit Price (BUY)} = P_{exit} = P_t *(1 - \frac{C}{100} * {M_c}^{t-1} )$$

Use this excel https://github.com/XHighIntell/Opportunity/raw/master/Math.xlsx
Use this chart https://www.desmos.com/calculator/brvdqgvvvt

Where
*t* = `The number of attempts, the first order will be 1`