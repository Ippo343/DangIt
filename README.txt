DANG IT! A Random Failures Mod for Kerbal Space Program

Poorly cobbled together by Ippo: you can contact him at m.ippolito@outlook.com

Please note that at this time, this mod is still unfinished and untested.
While I don't expect it to cause any trouble to your game or system, I cannot give you any guarantee.

This mod requires ModuleManager 2.1 or above. I have not tested it with earlier versions, nor I will: let me know if you find out something about it.


THE FAILURE MODEL


Every part that has a failure module attached will have a Mean Time Between Failures, or MTBF. This value, measured in hours, tells you how often you can expect to see the part fail when it is new.

Over time, the part will age and thus its MTBF will decrease exponentially.
The LifeTime of the part (again, in hours) is the time constant of this exponential decay: for those who are not really into math, this means that:
- after   LifeTime hours, the MTBF will have decreased by 70%;
- after 3*LifeTime hours, the MTBF has decreased so much that the use becomes impractical;
- after 5*LifeTime hours, the MTBF will be so small that it is essentially zero, thus making the part completely unusable.

At this moment, there is no way to extend the life of a part beyond the limit set in the cfg file.


Some parts only age when they are in use: for example, you can leave an engine off forever and it will be still as good as new: its age will only increase when you are throttling up.
Other parts, like tanks or reaction wheels, are always active, and therefore always aging: this is why they have a LifeTime so much higher than engines, because otherwise years-long missions would be impossible.


THE CFG FILES

The aging properties of a module are controlled in the included cfg files.
You can edit:
- MTBF: sets the mean time between failures of the part when it is new;
- LifeTime: sets the useful life of the part (see explanation above);
- UpdateInterval: the update interval in seconds. The default values should be good enough: parts that age only when active should have short update intervals to act accurately.
- AgeOnlyWhenActive: don't change this value or bugs might happen, because of reasons.