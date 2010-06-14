/* Yet Another Forum.NET
 * Copyright (C) 2006-2010 Jaben Cargman
 * http://www.yetanotherforum.net/
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 */
namespace YAF.Controls
{
  #region Using

  using System;
  using System.Web;
  using System.Web.UI;
  using System.Web.UI.WebControls;

  using YAF.Classes;
  using YAF.Classes.Core;
  using YAF.Classes.Utils;

  #endregion

  /// <summary>
  /// Summary description for Pager.
  /// </summary>
  public class Pager : BaseControl, IPostBackEventHandler
  {
    #region Constants and Fields

    /// <summary>
    ///   The _goto page form.
    /// </summary>
    private readonly GotoPageForm _gotoPageForm = new GotoPageForm();

    /// <summary>
    ///   The _page label.
    /// </summary>
    private readonly Label _pageLabel = new Label();

    /// <summary>
    ///   The _ignore page index.
    /// </summary>
    private bool _ignorePageIndex;

    /// <summary>
    ///   The _use post back.
    /// </summary>
    private bool _usePostBack = true;

    #endregion

    #region Events

    /// <summary>
    ///   The page change.
    /// </summary>
    public event EventHandler PageChange;

    #endregion

    #region Properties

    /// <summary>
    ///   Gets or sets Count.
    /// </summary>
    public int Count
    {
      get
      {
        if (this.ViewState["Count"] != null)
        {
          return (int)this.ViewState["Count"];
        }
        else
        {
          return 0;
        }
      }

      set
      {
        this.ViewState["Count"] = value;
      }
    }

    /// <summary>
    ///   Gets or sets CurrentPageIndex.
    /// </summary>
    public int CurrentPageIndex
    {
      get
      {
        return Convert.ToInt32(this.ViewState["CurrentPageIndex"] ?? 0);
      }

      set
      {
        this.ViewState["CurrentPageIndex"] = value;
      }
    }

    /// <summary>
    ///   Gets or sets LinkedPager.
    /// </summary>
    public string LinkedPager
    {
      get
      {
        return (string)this.ViewState["LinkedPager"];
      }

      set
      {
        this.ViewState["LinkedPager"] = value;
      }
    }

    /// <summary>
    ///   Gets PageCount.
    /// </summary>
    public int PageCount
    {
      get
      {
        return (int)Math.Ceiling((double)this.Count / this.PageSize);
      }
    }

    /// <summary>
    ///   Gets or sets PageSize.
    /// </summary>
    public int PageSize
    {
      get
      {
        if (this.ViewState["PageSize"] != null)
        {
          return (int)this.ViewState["PageSize"];
        }
        else
        {
          return 20;
        }
      }

      set
      {
        this.ViewState["PageSize"] = value;
      }
    }

    /// <summary>
    /// Gets the current item index.
    /// </summary>
    public int SkipIndex
    {
      get
      {
        return (int)Math.Ceiling((double)this.CurrentPageIndex * this.PageSize);
      }
    }

    /// <summary>
    ///   Gets or sets a value indicating whether UsePostBack.
    /// </summary>
    public bool UsePostBack
    {
      get
      {
        return this._usePostBack;
      }

      set
      {
        this._usePostBack = value;
      }
    }

    /// <summary>
    ///   Gets CurrentLinkedPager.
    /// </summary>
    /// <exception cref = "Exception">
    /// </exception>
    protected Pager CurrentLinkedPager
    {
      get
      {
        if (this.LinkedPager != null)
        {
          var linkedPager = (Pager)this.Parent.FindControl(this.LinkedPager);

          if (linkedPager == null)
          {
            throw new Exception(string.Format("Failed to link pager to '{0}'.", this.LinkedPager));
          }

          return linkedPager;
        }

        return null;
      }
    }

    #endregion

    #region Implemented Interfaces

    #region IPostBackEventHandler

    /// <summary>
    /// The raise post back event.
    /// </summary>
    /// <param name="eventArgument">
    /// The event argument.
    /// </param>
    public void RaisePostBackEvent(string eventArgument)
    {
      if (this.LinkedPager != null)
      {
        // raise post back event on the linked pager...
        this.CurrentLinkedPager.RaisePostBackEvent(eventArgument);
      }
      else if (this.PageChange != null)
      {
        this.CurrentPageIndex = int.Parse(eventArgument) - 1;
        this._ignorePageIndex = true;
        this.PageChange(this, new EventArgs());
      }
    }

    #endregion

    #endregion

    #region Methods

    /// <summary>
    /// The copy pager settings.
    /// </summary>
    /// <param name="toPager">
    /// The to pager.
    /// </param>
    protected void CopyPagerSettings(Pager toPager)
    {
      toPager.Count = this.Count;
      toPager.CurrentPageIndex = this.CurrentPageIndex;
      toPager.PageSize = this.PageSize;
    }

    /// <summary>
    /// The get page url.
    /// </summary>
    /// <param name="page">
    /// The page.
    /// </param>
    /// <returns>
    /// The get page url.
    /// </returns>
    protected string GetPageURL(int page)
    {
      string url = string.Empty;

      // create proper query string...
      var parser = new SimpleURLParameterParser(HttpContext.Current.Request.QueryString.ToString());

      // get the current page
      var currentPage = (ForumPages)Enum.Parse(typeof(ForumPages), parser["g"], true);

      if (parser["m"] != null)
      {
        // must be converted to by topic...
        parser.Parameters.Remove("m");
        parser.Parameters.Add("t", YafContext.Current.PageTopicID.ToString());
      }

      if (page > 1)
      {
        string tmp = parser.CreateQueryString(new[] { "g", "p", "tabid", "find" });
        if (tmp.Length > 0)
        {
          tmp += "&";
        }

        tmp += "p={0}";

        url = YafBuildLink.GetLink(currentPage, tmp, page);
      }
      else
      {
        url = YafBuildLink.GetLink(currentPage, parser.CreateQueryString(new[] { "g", "p", "tabid", "find" }));
      }

      return url;
    }

    /// <summary>
    /// The on init.
    /// </summary>
    /// <param name="e">
    /// The e.
    /// </param>
    protected override void OnInit(EventArgs e)
    {
      base.OnInit(e);

      if (!this._ignorePageIndex && HttpContext.Current.Request.QueryString["p"] != null)
      {
        // set a new page...
        this.CurrentPageIndex = (int)Security.StringToLongOrRedirect(HttpContext.Current.Request.QueryString["p"]) - 1;
      }

      this._pageLabel.ID = this.GetExtendedID("PageLabel");
      this._gotoPageForm.ID = this.GetExtendedID("GotoPageForm");

      this.Controls.Add(this._pageLabel);
      this.Controls.Add(this._gotoPageForm);

      // hook up events...
      this._gotoPageForm.GotoPageClick += this._gotoPageForm_GotoPageClick;
    }

    /// <summary>
    /// The on load.
    /// </summary>
    /// <param name="e">
    /// The e.
    /// </param>
    protected override void OnLoad(EventArgs e)
    {
      base.OnLoad(e);

      // init the necessary js...
      this.PageContext.PageElements.RegisterJQuery();
      this.PageContext.PageElements.RegisterJsResourceInclude("yafjs", "js/yaf.js");

      this.PageContext.PageElements.RegisterCssBlock("PagerCss", "#simplemodal-overlay {background-color:#000;}");
      this._pageLabel.Attributes.Add("style", "cursor: pointer");

      string modalFunction =
        String.Format(
          @"jQuery.fn.getBox = function() {{
  return {{
    left: $(this).offset().left,
    top: $(this).offset().top,
    width: $(this).outerWidth(),
    height: $(this).outerHeight()
  }};
}};

var gotoForumSuppressClick = false;

openGotoPageClick = function(e) {{
  return false;
}};

openGotoPageForm = function(id) {{
var labelBox = jQuery('#' + id).getBox();
var modalBox = jQuery('#{0}').getBox();
var gotoForm = jQuery('#{0}');

gotoForm.css({{position:'absolute',zindex:999,top:labelBox.top+labelBox.height,left:labelBox.left}});
gotoForm.fadeIn( 'slow', function() {{
	jQuery('#{0}').bind('click', openGotoPageClick);  
	jQuery(document).bind('click', function(e) {{
		jQuery('#{0}').hide();
		var fn = arguments.callee;
		jQuery(document).unbind('click', fn);
		jQuery('#{0}').unbind('click', openGotoPageClick);
	}});
  jQuery('#{1}').focus();
}});
}};
", 
          this._gotoPageForm.ClientID, 
          this._gotoPageForm.GotoTextBoxClientID);

      // register...
      this.PageContext.PageElements.RegisterJsBlock("OpenGotoPageFormJs", modalFunction);
      this.PageContext.PageElements.RegisterJsBlockStartup(
        String.Format(@"LoadPagerForm_{0}", this.ClientID), 
        String.Format(
          @"Sys.Application.add_load(function() {{ jQuery('#{0}').click(function() {{ openGotoPageForm('{0}'); }}); }});", 
          this._pageLabel.ClientID));
    }

    /// <summary>
    /// The render.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    protected override void Render(HtmlTextWriter output)
    {
      if (this.LinkedPager != null)
      {
        // just copy the linked pager settings but still render in this function...
        this.CurrentLinkedPager.CopyPagerSettings(this);
      }

      if (this.PageCount < 2)
      {
        return;
      }

      output.WriteLine(String.Format(@"<div class=""yafpager"" id=""{0}"">", this.ClientID));

      this._pageLabel.CssClass = "pagecount";

      // have to be careful about localization because the pager is used in the admin pages...
      string pagesText = "Pages";
      if (!String.IsNullOrEmpty(this.PageContext.Localization.TransPage))
      {
        pagesText = this.PageContext.Localization.GetText("COMMON", "PAGES");
      }

      this._pageLabel.Text = String.Format(@"{0:N0} {1}", this.PageCount, pagesText);

      // render this control...
      this._pageLabel.RenderControl(output);

      this.OutputLinks(output, this.UsePostBack);

      this._gotoPageForm.RenderControl(output);

      output.WriteLine("</div>");

      // base.Render( output );
    }

    /// <summary>
    /// The get link url.
    /// </summary>
    /// <param name="pageNum">
    /// The page num.
    /// </param>
    /// <param name="postBack">
    /// The post back.
    /// </param>
    /// <returns>
    /// The get link url.
    /// </returns>
    private string GetLinkUrl(int pageNum, bool postBack)
    {
      if (postBack)
      {
        return this.Page.ClientScript.GetPostBackClientHyperlink(this, pageNum.ToString());
      }

      return this.GetPageURL(pageNum);
    }

    /// <summary>
    /// The output links.
    /// </summary>
    /// <param name="output">
    /// The output.
    /// </param>
    /// <param name="postBack">
    /// The post back.
    /// </param>
    private void OutputLinks(HtmlTextWriter output, bool postBack)
    {
      int iStart = this.CurrentPageIndex - 2;
      int iEnd = this.CurrentPageIndex + 3;
      if (iStart < 0)
      {
        iStart = 0;
      }

      if (iEnd > this.PageCount)
      {
        iEnd = this.PageCount;
      }

      if (iStart > 0)
      {
        output.WriteBeginTag("span");
        output.WriteAttribute("class", "pagelinkfirst");
        output.Write(HtmlTextWriter.TagRightChar);

        this.RenderAnchorBegin(output, this.GetLinkUrl(1, postBack), null, "Go to First Page");

        output.Write("&laquo;");
        output.WriteEndTag("a");
        output.WriteEndTag("span");
      }

      if (this.CurrentPageIndex > iStart)
      {
        output.WriteBeginTag("span");
        output.WriteAttribute("class", "pagelink");
        output.Write(HtmlTextWriter.TagRightChar);

        this.RenderAnchorBegin(output, this.GetLinkUrl(this.CurrentPageIndex, postBack), null, "Prev Page");

        output.Write("&lt;");
        output.WriteEndTag("a");
        output.WriteEndTag("span");
      }

      for (int i = iStart; i < iEnd; i++)
      {
        if (i == this.CurrentPageIndex)
        {
          output.WriteBeginTag("span");
          output.WriteAttribute("class", "pagecurrent");
          output.Write(HtmlTextWriter.TagRightChar);
          output.Write(i + 1);
          output.WriteEndTag("span");
        }
        else
        {
          string page = (i + 1).ToString();

          output.WriteBeginTag("span");
          output.WriteAttribute("class", "pagelink");
          output.Write(HtmlTextWriter.TagRightChar);

          this.RenderAnchorBegin(output, this.GetLinkUrl(i + 1, postBack), null, page);

          output.Write(page);
          output.WriteEndTag("a");
          output.WriteEndTag("span");
        }
      }

      if (this.CurrentPageIndex < (this.PageCount - 1))
      {
        output.WriteBeginTag("span");
        output.WriteAttribute("class", "pagelink");
        output.Write(HtmlTextWriter.TagRightChar);

        this.RenderAnchorBegin(output, this.GetLinkUrl(this.CurrentPageIndex + 2, postBack), null, "Next Page");

        output.Write("&gt;");
        output.WriteEndTag("a");
        output.WriteEndTag("span");
      }

      if (iEnd < this.PageCount)
      {
        output.WriteBeginTag("span");
        output.WriteAttribute("class", "pagelinklast");
        output.Write(HtmlTextWriter.TagRightChar);

        this.RenderAnchorBegin(output, this.GetLinkUrl(this.PageCount, postBack), null, "Go to Last Page");

        output.Write("&raquo;");
        output.WriteEndTag("a");
        output.WriteEndTag("span");
      }
    }

    /// <summary>
    /// The _goto page form_ goto page click.
    /// </summary>
    /// <param name="sender">
    /// The sender.
    /// </param>
    /// <param name="e">
    /// The e.
    /// </param>
    private void _gotoPageForm_GotoPageClick(object sender, GotoPageForumEventArgs e)
    {
      int newPage = e.GotoPage - 1;

      if (newPage >= 0 && newPage < this.PageCount)
      {
        // set a new page index...
        this.CurrentPageIndex = newPage;
        this._ignorePageIndex = true;
      }

      if (this.LinkedPager != null)
      {
        // raise post back event on the linked pager...
        this.CurrentLinkedPager._gotoPageForm_GotoPageClick(sender, e);
      }
      else if (this.PageChange != null)
      {
        this.PageChange(this, new EventArgs());
      }
    }

    #endregion
  }
}