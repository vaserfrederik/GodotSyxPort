package settlement.room.tests;

import java.util.Arrays;

import game.faction.npc.stockpile.NPCStockpile;
import init.resources.RESOURCE;
import init.resources.RESOURCES;
import init.sprite.UI.UI;
import settlement.main.SETT;
import settlement.room.industry.module.Industry;
import settlement.room.industry.module.IndustryResource;
import settlement.room.tests.production.Input;
import settlement.room.tests.production.Production;
import settlement.room.tests.production.ProductionSpec;
import settlement.room.tests.production.ProductionTrader;
import settlement.room.tests.production.TestRecipe;
import snake2d.LOG;
import snake2d.SPRITE_RENDERER;
import snake2d.util.color.COLOR;
import snake2d.util.datatypes.DIR;
import snake2d.util.gui.GUI_BOX;
import snake2d.util.gui.GuiSection;
import snake2d.util.gui.Hoverable.HOVERABLE;
import snake2d.util.gui.renderable.RENDEROBJ;
import snake2d.util.sets.LIST;
import snake2d.util.sprite.text.Str;
import util.data.GETTER;
import util.data.INT.INTE;
import util.data.INT.IntImp;
import util.gui.misc.GBox;
import util.gui.misc.GButt;
import util.gui.misc.GStat;
import util.gui.misc.GText;
import util.gui.slider.GSliderInt;
import util.gui.table.GTableBuilder;
import util.gui.table.GTableBuilder.GRowBuilder;
import util.info.GFORMAT;
import util.text.D;
import util.text.Dic;

class UITradeDebug extends GuiSection{

	/**
	 * 
	 * Game Theory & toll
	 * Game theory counteracts toll. Initially, these should be the same. With progress,
	 * toll increases, making common goods less potent.
	 * With time, toll can be decreased, favouring crappy goods.
	 * 
	 * 
	 * 
	 */

	
	private final int credits = NPCStockpile.AVERAGE_PRICE;
	private final IntImp toll = new IntImp(0, 1000);
	private final IntImp gametheory = new IntImp(0, 100) {
		
		@Override
		public void set(int t) {
			super.set(t);
			pbonuses.recount();
		};
		
	};
	private final IntImp flatTariffSell = new IntImp(0, 100);
	private final IntImp flatTariffBuy = new IntImp(0, 100);
	private final IntImp bonusai = new IntImp(100, 0, 600);
	private final IntImp wPerItemUSed = new IntImp(0, 0, 20);
	private final IntImp consumptionBonus = new IntImp(0, 0, 100);
	
	
	private final ProductionSpec aibonuses = new ProductionSpec() {
		
		@Override
		public double bonus(Industry t) {
			return bonusai.get()/100.0;
		}

		@Override
		public double consumptionBonus(Industry ins) {
			return 1.0;
		}

		@Override
		public double wPerItemUsed() {
			return 0.1;
		}

		@Override
		public double addedW() {
			return gametheory.getD();
		}

	};
	private final Production data  = new Production();
	private final ProductionTrader trade  = new ProductionTrader(data, credits);
	
	private class BonusPlayer implements ProductionSpec {
		private final double[] bonus = new double[Industry.all().size()];
		private final double[] cons = new double[Industry.all().size()];

		final ProductionSpec spec = new ProductionSpec() {
			
			@Override
			public double wPerItemUsed() {
				return 0.05;
			}
			
			@Override
			public double consumptionBonus(Industry ins) {
				return 1;
			}
			
			@Override
			public double bonus(Industry ins) {
				return 1;
			}
			
			@Override
			public double addedW() {
				return 0.05;
			}

		};
		{
			clear();
		}
		
		BonusPlayer(){
			recount();
		}
		
		private void recount() {
			

			
			final double toll = 0.05;
			final double buyT = 0.0;
			final double sellT = 0.2; // 0.15 is ally
			
			Arrays.fill(cons, 1.0);
			Arrays.fill(bonus, 1.0);
			bonusai.set(100);
			
			for (RESOURCE r : RESOURCES.ALL()) {
				LOG.ln(r.key + " " + (int)(credits*data.price(r, spec)));
				LOG.ln("buy " + (int)(credits*trade.sellPrice(r, spec, 1, buyT, toll)));
				LOG.ln("sell " + (int)(credits*trade.buyPrice(r, spec, 1, sellT, toll)));
				LIST<TestRecipe> rs = data.get(r);
			
				for (TestRecipe rr : rs) {
					double t = rr.pricePerItem(spec);
					LOG.ln("    " + t + " " + rr.rate + " " + rr.wPerItem(spec));
						
				}
			}
			
			
			
			boolean change = true;
			while(change) {
				change = false;
				
				
				
				for (Industry ins : SETT.ROOMS().industries.all) {
					
					
					for (TestRecipe r : data.get(ins)) {
						
						
						int expenditure = 0;
						for (Input i : r.inputs()) {
							double amount = spec.bonus(ins)*i.amount;
							expenditure += trade.buyPrice(i.res, spec, amount, buyT, toll);
						}

						
						
						double amount = spec.bonus(ins)*r.rate;
						
						int income = trade.sellPrice(r.res, spec, amount, sellT, toll);
					
						double cr = credits - amount*toll - credits*sellT;
						
						if (expenditure > 0) {
							//income - expenditure * c = credits;
							//income = credits + expenditure * c
							//income - credits = expenditure * c
							//(income - credits)/expenditure = c;
							double c = (income-cr)/(double)expenditure;
							LOG.ln(ins.blue.key + " " + r.rate + " " + income + " " + -expenditure + " " + c + " " + (c < 1));
							if (c < 1) {
								
								c = 1.0/c;
								if (c > cons[ins.index()]) {
									cons[ins.index()] = c;
									change = true;
								}
							}
						}
						
						
					}
					
				}
			}
			LOG.ln();
			for (Industry ins : SETT.ROOMS().industries.all) {
				if (ins.outs().size() == 0)
					continue;
				String s = " | " + ins.outs().get(0).resource.name + " -> ";
				for (IndustryResource ii : ins.ins())
					s += ii.resource.name + ", ";
				LOG.ln(ins.blue.key + " " + ((int)((cons[ins.index()]-1)*100))/100.0 + s);
				
			}
			
			for (Industry ins : SETT.ROOMS().industries.all) {
				if (ins.outs().size() == 0)
					continue;
				String s = " | " + ins.outs().get(0).resource.name + " -> ";
				for (IndustryResource ii : ins.ins())
					s += ii.resource.name + ", ";
				LOG.ln(ins.blue.key + " " + ((int)((cons[ins.index()]-1)*100))/100.0 + s);
				
			}
		}
		

		@Override
		public double bonus(Industry t) {
			return bonus[t.index()];
		}

		@Override
		public double consumptionBonus(Industry ins) {
			return 1*(1-consumptionBonus.getD()) + (cons[ins.index()]*consumptionBonus.getD());
		}
		
		void clear() {
			Arrays.fill(bonus, 1.0);
			
		}

		@Override
		public double wPerItemUsed() {
			return 0;
		}

		@Override
		public double addedW() {
			return gametheory.getD();
		}
		
	}
	private final BonusPlayer pbonuses = new BonusPlayer();
	
	
	
	private static CharSequence ¤¤work = "Work required/item";
	private static CharSequence ¤¤workT = "Total work required/item";
	
	static {
		D.ts(UITradeDebug.class);
	}
	
	UITradeDebug(){
		
		
		
		{
			
			GuiSection info = new GuiSection();
			
			GSliderInt i = new GSliderInt(toll, 100, false);
			i.hoverInfoSet(Dic.¤¤Toll);
			info.add(i);
			
			i = new GSliderInt(gametheory, 100, false);
			i.hoverInfoSet("Game theory");
			info.addRightC(48, i);
			
			i = new GSliderInt(flatTariffSell, 100, false);
			i.hoverInfoSet("sell tariff");
			info.addRightC(48, i);
			
			i = new GSliderInt(flatTariffBuy, 100, false);
			i.hoverInfoSet("buy tariff");
			info.addRightC(48, i);
			
			addRelBody(0, DIR.S, info);
			
			info = new GuiSection();
			
			INTE bb = new INTE() {
				
				@Override
				public int min() {
					return 0;
				}
				
				@Override
				public int max() {
					return 600;
				}
				
				@Override
				public int get() {
					return (int) (pbonuses.bonus[0]*100);
				}
				
				@Override
				public void set(int t) {
					Arrays.fill(pbonuses.bonus, t/100.0);
					
				}
			};
			
			i = new GSliderInt(bb, 100, false);
			i.hoverInfoSet("bonus player");
			info.addRightC(48, i);
			
			i = new GSliderInt(bonusai, 100, false);
			i.hoverInfoSet("bonus ai");
			info.addRightC(48, i);
			
			i = new GSliderInt(wPerItemUSed, 100, false);
			i.hoverInfoSet("w per total items used");
			info.addRightC(48, i);
			
			i = new GSliderInt(consumptionBonus, 100, false);
			i.hoverInfoSet("consumption bonus");
			info.addRightC(48, i);
			
			addRelBody(0, DIR.S, info);
			
		}
		
		{
			GuiSection info = new GuiSection();
			
			info.addRightC(0, new GButt.ButtPanel("beginning") {
				@Override
				protected void clickA() {
					toll.set(18);
					gametheory.set(0);
					flatTariffSell.set(30);
					flatTariffBuy.set(0);
					pbonuses.clear();
					bonusai.set(300);
					wPerItemUSed.set(10);
					consumptionBonus.set(0);
					
					super.clickA();
				}
			});
			
			info.addRightC(0, new GButt.ButtPanel("start") {
				@Override
				protected void clickA() {
					toll.set(100);
					gametheory.set(20);
					flatTariffSell.set(20);
					flatTariffBuy.set(20);
					pbonuses.clear();
					bonusai.set(300);
					super.clickA();
				}
			});
			
			info.addRightC(0, new GButt.ButtPanel("relations") {
				@Override
				protected void clickA() {
					toll.set(100);
					gametheory.set(20);
					flatTariffSell.set(5);
					flatTariffBuy.set(5);
					pbonuses.clear();
					bonusai.set(300);
					super.clickA();
				}
			});
			
			info.addRightC(0, new GButt.ButtPanel("toll & relations") {
				@Override
				protected void clickA() {
					toll.set(25);
					gametheory.set(20);
					flatTariffSell.set(5);
					flatTariffBuy.set(5);
					pbonuses.clear();
					bonusai.set(300);
					super.clickA();
				}
			});
			info.addRightC(0, new GButt.ButtPanel("research") {
				@Override
				protected void clickA() {
					toll.set(25);
					gametheory.set(20);
					flatTariffSell.set(5);
					flatTariffBuy.set(5);
					bonusai.set(100);
					
					pbonuses.clear();
					Arrays.fill(pbonuses.bonus, 0);
					double hi = 0;
					for (Industry ins : SETT.ROOMS().industries.all) {
						
						for (TestRecipe r : data.get(ins)) {
							double c = r.amountPerW(aibonuses)/r.amountPerWTot(aibonuses);
							if (c > pbonuses.bonus[ins.index()])
								pbonuses.bonus[ins.index()] = c;
							hi = Math.max(c, hi);
						}
						
					}
					for (Industry ins : Industry.all()) {
						
						pbonuses.bonus[ins.index()] /= hi;
						pbonuses.bonus[ins.index()]*=6;
						pbonuses.bonus[ins.index()] += 1;
						LOG.ln(ins.blue + " " + (-1 + pbonuses.bonus[ins.index()]));
						
					}
					
					
					bonusai.set(300);
					
					super.clickA();
				}
				
			});
			info.addRightC(0, new GButt.ButtPanel("consumption") {
				@Override
				protected void clickA() {
					toll.set(18);
					gametheory.setD(0.05);
					flatTariffSell.set(30);
					flatTariffBuy.set(0);
					pbonuses.clear();
					bonusai.set(300);
					wPerItemUSed.set(10);
					consumptionBonus.set(100);
					pbonuses.recount();
					
					super.clickA();
				}
				
			});
			
			info.add(new GButt.ButtPanel("old system") {
				@Override
				protected void clickA() {
					toll.set(0);
					gametheory.set(0);
					flatTariffSell.set(0);
					flatTariffBuy.set(0);
					pbonuses.clear();
					bonusai.set(100);
					super.clickA();
				}
			}, info.body().x1(), info.body().y2());
			
			addRelBody(0, DIR.S, info);
		}
		
		
		
		
		GTableBuilder bu = new GTableBuilder() {
			
			@Override
			public int nrOFEntries() {
				return data.all().size();
			}
		};
		
		
		bu.column(Dic.¤¤Production, 500, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				
				return new HOVERABLE.HoverableAbs(500, 32) {

					@Override
					protected void render(SPRITE_RENDERER ren, float ds, boolean isHovered) {
						TestRecipe r = get(ier);
						
						int x1 = body.x1()+4;
						
						UI.FONT().S.renderCY(ren, x1, body.cY(), Str.TMP.clear().add(1.0/r.wPerItem, 1));
						x1 += 32;
						
						r.res.icon().renderCY(ren, x1, body.cY());
						x1+= 32;
						
						
						r.ins.blue.icon.renderCY(ren, x1, body.cY());
						COLOR.BLACK.bind();
						UI.FONT().S.renderC(ren,  x1+8+2, body.cY()-8+2, GFORMAT.toNumeral(r.index+1));
						UI.FONT().S.renderC(ren,  x1+8-2, body.cY()-8-2, GFORMAT.toNumeral(r.index+1));
						COLOR.unbind();
						UI.FONT().S.renderC(ren, x1+8, body.cY()-8, GFORMAT.toNumeral(r.index+1));
						x1 += 28;
						UI.icons().s.arrow_left.renderCY(ren, x1, body.cY());
						x1 += 24;
						
						
						for (int i = 0; i < r.inputs().size(); i++) {
							
							Input ii = r.inputs().get(i);
							UI.FONT().S.renderCY(ren, x1, body.cY(), Str.TMP.clear().add(ii.amount, 1));
							x1 += 32;
							ii.producer.ins.blue.icon.renderCY(ren, x1, body.cY());
							ii.res.icon().renderCY(ren, x1+24, body.cY()-8);
							COLOR.BLACK.bind();
							UI.FONT().S.renderC(ren, x1+24+2, body.cY()-8+2, GFORMAT.toNumeral(ii.producer.index+1));
							UI.FONT().S.renderC(ren, x1+24-2, body.cY()-8-2, GFORMAT.toNumeral(ii.producer.index+1));
							COLOR.unbind();
							UI.FONT().S.renderC(ren, x1+24, body.cY()-8, GFORMAT.toNumeral(ii.producer.index+1));
							x1 += 48;
							if (x1 > body.x2()-100)
								break;
						}
						
						
						
					}
					
					@Override
					public void hoverInfoGet(GUI_BOX text) {
						TestRecipe r = get(ier);
						GBox b = (GBox) text;
						b.title(b.text().add(r.ins.blue.info.name).add(':').s().add(r.res.name).s().add(GFORMAT.toNumeral(r.index+1)));
						
						b.textLL(¤¤work);
						b.tab(6);
						b.add(GFORMAT.f(b.text(), r.wPerItem(pbonuses)));
						b.NL();
						b.textLL(¤¤workT);
						b.tab(6);
						b.add(GFORMAT.f(b.text(), r.wTotPerItem(pbonuses)));
						b.NL();
						
						hrec(b, 0, r.inputs());
						
						
						super.hoverInfoGet(text);
					}
					
					private void hrec(GBox b, int tab, LIST<Input> inputs) {
						if (inputs.size() == 0)
							return;
						
						for (int i = 0; i < inputs.size(); i++) {
							b.tab(tab);
							Input ii = inputs.get(i);
							b.add(GFORMAT.f(b.text(), ii.amount, 2));
							b.add(ii.res.icon());
							b.add(ii.producer.ins.blue.icon.medium);
							b.space();
							b.NL();
							hrec(b, tab+1, ii.producer.inputs());
							b.NL(8);
						}
					}
				};
			}
		});
		
		final int colW = 100;
		
		bu.column("A", colW, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				
				return new GStat() {
					
					@Override
					public void update(GText text) {
						TestRecipe r = get(ier);
						if (r.inputs().size() == 0)
							return;
						
						
						GFORMAT.iIncr(text, A(r));
						
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						b.text("Produce all inputs and sell them");
						b.NL();
						TestRecipe r = get(ier);
						if (r.inputs().size() == 0)
							return;
						
						double tot = 0;
						for (Input i : r.inputs()) {
							tot += i.amount;
						}
						
						int sold = 0;
						
						trade.hoverLegend(b);
						
						for (Input i : r.inputs()) {
							double am = i.amount/(tot*i.producer.wTotPerItem(pbonuses));
							trade.hoverSale(b, i.res, aibonuses, am, flatTariffSell.getD(), toll.getD());
							
							int s = trade.sellPrice(i.res, aibonuses, am, flatTariffSell.getD(), toll.getD());
							sold += s;
							b.NL();
						}
						
						trade.hoverSum(b, sold);
					};
					
				}.r(DIR.NE);
			}
		}, DIR.NE);
		
		bu.column("B", colW, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				return new GStat() {
					
					@Override
					public void update(GText text) {
						TestRecipe r = get(ier);
						GFORMAT.iIncr(text, B(r));
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						
						TestRecipe r = get(ier);
						
						double d = r.amountPerWTot(pbonuses);
						d = (d-r.amountPerW(pbonuses))/2;
						
						for (Input i : r.inputs()) {
							double a = i.producer.amountPerWTot(pbonuses)/i.amount;
							b.add(GFORMAT.f(b.text(), -a, 2));
							b.tab(2);
							b.add(i.res.icon());
							b.NL();
							
							
						}
						
						b.add(r.res.icon());
						b.add(GFORMAT.f(b.text(), r.amountPerWTot(pbonuses)));
						b.NL();
						
						trade.hoverLegend(b);
						double am = r.amountPerWTot(pbonuses);
						trade.hoverSale(b, r.res, aibonuses, am, flatTariffSell.getD(), toll.getD());
						
					};
					
				}.r(DIR.NE);
			}
		}, DIR.NE);
		
		bu.column("C", colW, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				return new GStat() {
					
					@Override
					public void update(GText text) {
						TestRecipe r = get(ier);
						
						if (r.inputs().size() == 0)
							return;
						GFORMAT.iIncr(text, C(r));
						
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						
						
						TestRecipe r = get(ier);
						if (r.inputs().size() == 0)
							return;
						trade.hoverLegend(b);
						double am = pbonuses.bonus(r.ins)/r.wPerItem(pbonuses);
						int in = trade.sellPrice(r.res, aibonuses, r.amountPerW(pbonuses), flatTariffSell.getD(), toll.getD());
						int out = 0;
						for (Input i : r.inputs()) {
							double a = i.amount*pbonuses.bonus(r.ins)/pbonuses.consumptionBonus(r.ins);
							trade.hoverPuchase(b, i.res, aibonuses, a, flatTariffBuy.getD(), toll.getD());
							out += trade.buyPrice(i.res, aibonuses, a, flatTariffBuy.getD(), toll.getD());
						}
						
						trade.hoverSale(b, r.res, aibonuses, am, flatTariffSell.getD(), toll.getD());
						trade.hoverSum(b, in-out);
						
					};
					
				}.r(DIR.NE);
			}
		}, DIR.NE);
		
		bu.column("buy", colW, new GRowBuilder() {
			
			@Override
			public RENDEROBJ build(GETTER<Integer> ier) {
				return new GStat() {
					
					@Override
					public void update(GText text) {
						TestRecipe r = get(ier);
						double p = trade.buyPrice(r.res, aibonuses, r.amountPerW(aibonuses), flatTariffBuy.getD(), toll.getD());

						GFORMAT.iIncr(text,-(long) p);
						
					}
					
					@Override
					public void hoverInfoGet(GBox b) {
						
						
					};
					
				}.r(DIR.NE);
			}
		}, DIR.NE);
		
		addRelBody(8, DIR.S, bu.create(10, true));
		
	}

	public int A(TestRecipe r) {
		if (r.inputs().size() == 0)
			return 0;
		
		double tot = 0;
		for (Input i : r.inputs()) {
			tot += i.amount;
		}
		
		int sold = 0;
		for (Input i : r.inputs()) {
			double am = i.amount/(tot*i.producer.wTotPerItem(pbonuses));
			sold += trade.sellPrice(i.res, aibonuses,  am, flatTariffSell.getD(), toll.getD());
		}
		return sold;
	}
	
	public int B(TestRecipe r) {
		double tot = r.amountPerWTot(pbonuses);
		return trade.sellPrice(r.res, aibonuses, tot, flatTariffSell.getD(), toll.getD());
	}

	public int C(TestRecipe r) {
		
		if (r.inputs().size() == 0)
			return 0;
		
		int in = trade.sellPrice(r.res, aibonuses, r.amountPerW(pbonuses), flatTariffSell.getD(), toll.getD());
		
		int out = 0;
		for (Input i : r.inputs()) {
			out += trade.buyPrice(i.res, aibonuses, i.amount*pbonuses.bonus(r.ins)/pbonuses.consumptionBonus(r.ins), flatTariffBuy.getD(), toll.getD());
		}
		
		return in-out;
	}
	
	TestRecipe get(GETTER<Integer> ier) {
		return data.all().get(ier.get());
	}
	
	
}
