using System;
using System.Collections.Generic;
using System.Linq;

public class UIBuckets : GuiSection
{
    private readonly int WIDTH;
    private readonly int HEIGHT;
    private UIBucketsCard dragging;
    private Column draggingTo;

    private readonly List<Card> cards;
    private readonly List<Column> columns;
    private readonly Column[] toSort;

    private bool dirty = true;

    private readonly IComparer<Column> sorter = new ColumnComparer();

    public UIBuckets(int width, List<SPRITE> bucketSprites, List<UIBucketsCard> cardSprites)
    {
        WIDTH = width;
        cards = new List<Card>(cardSprites.Count);
        columns = new List<Column>(bucketSprites.Count);
        toSort = new Column[bucketSprites.Count];
        foreach (UIBucketsCard g in cardSprites)
        {
            cards.Add(new Card(g));
        }

        HEIGHT = cards[0].body.height() + 16;

        int m = (int)Math.Ceiling(cards.Count / (1 + WIDTH / cards[0].body().width()));
        m = Math.Clamp(m, 0, 2);

        for (int i = 0; i < bucketSprites.Count; i++)
        {
            columns.Add(new Column(i, bucketSprites[i]));
        }

        Init();
    }

    public override void Render(SPRITE_RENDERER r, float ds)
    {
        if (dirty)
        {
            Init();
            dragging = null;
            draggingTo = null;
            dirty = false;
        }

        draggingTo = null;

        base.Render(r, ds);
        if (!MButt.LEFT.isDown())
        {
            if (draggingTo != null)
            {
                dragging.prio = draggingTo.pp;
                dirty = true;
            }
            dragging = null;
            draggingTo = null;
        }
        else if (dragging != null)
        {
            if (MButt.RIGHT.consumeClick())
            {
                dragging = null;
                draggingTo = null;
            }
            else
                dragging.s.renderC(r, VIEW.mouse());
        }
    }

    private void Init()
    {
        int x1 = body().x1();
        int y1 = body().y1();
        Clear();

        foreach (Column c in columns)
        {
            toSort[c.pp] = c;
            c.cards = 0;
            c.height = 0;
        }

        foreach (Card c in cards)
        {
            if (!IsActive(c.g))
                continue;
            toSort[c.g.prio].cards++;
        }

        for (int i = 0; i <= columns.Count - 1; i++)
        {
            Array.Sort(toSort, sorter);
            toSort[0].height++;
        }

        foreach (Column c in columns)
        {
            c.Init(cards);
            AddDown(2, c);
        }
        body().moveX1Y1(x1, y1);
    }

    private class Column : GuiSection
    {
        private readonly int pp;
        private readonly SPRITE icon;
        public int cards;
        public int height = 0;

        public Column(int prio, SPRITE icon)
        {
            this.pp = prio;
            body().setDim(WIDTH, HEIGHT);
            this.icon = icon;
        }

        public override void Render(SPRITE_RENDERER r, float ds)
        {
            bool hov = hoveredIs() & dragging != null && dragging.prio != pp;

            if (hov)
                draggingTo = this;
            GButt.ButtPanel.renderBG(r, true, hov, hov, body());

            GCOLOR.UI().border().renderFrame(r, body(), 0, 1);
            base.Render(r, ds);
            icon.renderC(r, body().x1() + 16, body().cY());
        }

        public void Init(List<Card> cards)
        {
            int am = 0;
            foreach (Card c in cards)
            {
                if (!IsActive(c.g))
                    continue;
                if (c.g.prio == pp)
                {
                    am++;
                }
            }

            int x1 = body().x1();
            int y1 = body().y1();

            Clear();
            body().setDim(WIDTH, HEIGHT + height * 32);

            if (am == 0)
                return;

            int WW = WIDTH - 32;

            int dist = (height + 1) * (WW - cards[0].body.width()) / (am);
            if (dist > cards[0].body.width())
                dist = cards[0].body.width();

            int sy = body().y1() + 8;
            int sx = body().x1() + 32;

            foreach (Card c in cards)
            {
                if (!IsActive(c.g))
                    continue;
                if (c.g.prio == pp)
                {
                    Add(c, sx, sy);
                    sx += dist;
                    if (sx + cards[0].body.width() + 8 > body().x2())
                    {
                        sy += 32;
                        sx = body().x1() + 32;
                    }
                }
            }

            body().moveX1Y1(x1, y1);
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            GBox b = (GBox)text;
            base.HoverInfoGet(b);
            if (b.EmptyIs())
            {
                HoverBucket(b, pp);
            }
        }
    }

    public void HoverBucket(GBox text, int bucket)
    {
    }

    public void HoverCard(GBox text, UIBucketsCard c)
    {
    }

    public bool IsActive(UIBucketsCard c)
    {
        return true;
    }

    private class Card : GButt.ButtPanel
    {
        private readonly UIBucketsCard g;

        public Card(UIBucketsCard g) : base(g.s)
        {
            this.g = g;
        }

        protected override void ClickA()
        {
            dragging = g;
        }

        protected override void RenAction()
        {
            selectedSet(dragging == g);
            if (dragging != null)
                isHovered = false;
            base.RenAction();
        }

        public override void HoverInfoGet(GUI_BOX text)
        {
            HoverCard((GBox)text, g);
        }
    }

    public void SetBucket(UIBucketsCard c, int bucket)
    {
        c.prio = Math.Clamp(bucket, 0, columns.Count - 1);
    }

    public class UIBucketsCard
    {
        public int prio = 0;
        public readonly SPRITE s;
        public readonly object o;

        public UIBucketsCard(SPRITE s, object o)
        {
            this.s = s;
            this.o = o;
        }

        public int prio()
        {
            return prio;
        }
    }

    private class ColumnComparer : IComparer<Column>
    {
        public int Compare(Column o1, Column o2)
        {
            int i1 = o1.cards - o1.height * cards.Count / (columns.Count);
            int i2 = o2.cards - o2.height * cards.Count / (columns.Count);
            return i2 - i1;
        }
    }
}