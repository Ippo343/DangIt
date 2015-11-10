using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ippo
{
	/// <summary>
	/// Helper class that can manage either a ModuleDecouple or ModulenchoredDecoupler
	/// </summary>
	public class DecouplerManager
	{
		public ModuleDecouple decoupler;
		public ModuleAnchoredDecoupler aDecoupler;
		public bool isDecoupler;

		/// <summary>
		/// Creates an DecouplerManager for a part.
		/// Allows unified control of a ModuleDecouple/ModulenchoredDecoupler.
		/// </summary>
		public DecouplerManager(Part p)
		{
			this.decoupler = p.Modules.OfType<ModuleDecouple>().FirstOrDefault();
			this.aDecoupler = p.Modules.OfType<ModuleAnchoredDecoupler>().FirstOrDefault();
			this.isDecoupler = this.decoupler != null;
		}

		/// <summary>
		/// True when there is any active engine module on the part.
		/// </summary>
		public float ejectionForcePercent{
			get{
				return this.isDecoupler ? this.decoupler.ejectionForcePercent : this.aDecoupler.ejectionForcePercent;
			}
			set{
				if (this.isDecoupler) {
					this.decoupler.ejectionForcePercent = value;
				} else {
					this.aDecoupler.ejectionForcePercent = value;
				}
			}
		}

	}
}
